using APIServer.Models.Entity;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<Result<(int gold, int gem)>> PurchaseCharacterAsync(
        long userId, long characterCode, int goldPrice, int gemPrice)
    {
        // 0) 입력 가드
        if (goldPrice < 0 || gemPrice < 0)
            return Result<(int gold, int gem)>.Failure(ErrorCode.InvalidPrice);

        // 1) 이미 보유 여부 확인
        if (await CheckAlreadyHaveCharacter(userId, characterCode) == false)
        {
            return Result<(int, int)>.Failure(ErrorCode.CannotFindCharacter);
        }

        // 2) 재화 조회
        var (currentGold, currentGem) = await GetGoldAndGem(userId);

        // 3) 구매 가능 여부
        var canPayGold = currentGold >= goldPrice;
        var canPayGem  = currentGem >= gemPrice;
        if (!canPayGold || !canPayGem)
            return Result<(int gold, int gem)>.Failure(ErrorCode.CannotPurchaseCharacter);

        // 4) 결과 값 계산
        var newGold = currentGold - goldPrice;
        var newGem  = currentGem  - gemPrice;

        // 5) 트랜잭션: 재화 차감 → 캐릭터 지급
        var code = await WithTransactionAsync(async _ =>
        {
            if (await UpdateGoldAndGem(userId, newGold, newGem) == false)
            {
                return ErrorCode.FailedUpdateData;
            }
            
            if(await InsertNewCharacterAsync(userId, characterCode) == false)
            {
                return ErrorCode.FailedInsertNewCharacter;
            }

            return ErrorCode.None;
        });

        if (code != ErrorCode.None)
            return Result<(int gold, int gem)>.Failure(code);

        // 6) 최신 잔액 반환
        return Result<(int gold, int gem)>.Success((newGold, newGem));
    }


    public async Task<ErrorCode> SellInventoryItemAsync(long userId, long itemId)
    {
        // 1) 아이템 조회
        var item = await GetInventoryItemAsync(userId, itemId);

        // 2) 장착 여부 확인
        if (await IsItemEquippedAsync(itemId))
        {
            return ErrorCode.CannotSellEquipmentItem;
        }

        // 3) 판매가 및 현재 잔액 조회
        var sellGold = _masterDb.GetItemEnhanceDatas()[(itemId, item.level)].sellPrice;
        var (curGold, curGem) = await GetUserCurrencyAsync(userId);

        // 4) 결과 잔액 계산 (반환은 안 하지만 로직/로그에 사용)
        var newGold = curGold + sellGold;
        var newGem  = curGem;

        // 5) 트랜잭션: 인벤 삭제 → 재화 갱신
        var code = await WithTransactionAsync(async _ =>
        {
            if (await DeleteInventoryItemAsync(userId, itemId) == false)
            {
                return ErrorCode.FailedDeleteInventoryItem;
            }

            if (await UpdateUserCurrencyAsync(userId, newGold, newGem) == false)
            {
                return ErrorCode.FailedUpdateUserGoldAndGem;
            }
            
            LogInfo(_logger, EventType.SellItem, "Sell Item Success",
                new { userId, itemId, beforeGold = curGold, beforeGem = curGem, newGold, newGem });

            return ErrorCode.None;
        });

        return code;
    }

    private async Task<UserInventoryItem> GetInventoryItemAsync(long userId, long itemId)
    {
        return await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .FirstOrDefaultAsync<UserInventoryItem>();
    }

    private async Task<UserInventoryRune> GetInventoryRuneAsync(long userId, long runeId)
    {
        return await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .FirstOrDefaultAsync<UserInventoryRune>();
    }
    
    private async Task<bool> IsItemEquippedAsync(long itemId)
    {
        var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }

    private async Task<bool> DeleteInventoryItemAsync(long userId, long itemId)
    {
        var affected = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .DeleteAsync();

        return affected == 1;
    }

    private async Task<(int gold, int gem)> GetUserCurrencyAsync(long userId)
    {
        return await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
    }

    private async Task<bool> UpdateUserCurrencyAsync(long userId, int gold, int gem)
    {
        var affected = await _queryFactory.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new { gold, gem });

        return affected == 1;
    }
    
    private async Task<bool> CheckAlreadyHaveCharacter(long userId, long characterCode)
    {
        var cnt = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
            .Where(USER_ID, userId)
            .Where(CHARACTER_CODE, characterCode)
            .CountAsync<long>();

        return cnt > 0;
    }
    
    private async Task<(int,int)> GetGoldAndGem(long userId)
    {
        return await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
    }

    private async Task<bool> UpdateGoldAndGem(long userId, int newGold, int newGem)
    {
        var result =  await _queryFactory.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                GOLD = newGold,
                GEM = newGem,
            });

        return result == 1;
    }

    private async Task<bool> InsertNewCharacterAsync(long userId, long characterCode)
    {
        var result =  await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
            .InsertAsync(new
            {
                USER_ID = userId,
                LEVEL = 1,
                CHARACTER_CODE = characterCode
            });

        return result == 1;
    }
}