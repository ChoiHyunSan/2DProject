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
        var alreadyHave = await CheckAlreadyHaveCharacter(userId, characterCode);
        if (alreadyHave.IsFailed)
            return Result<(int gold, int gem)>.Failure(alreadyHave.ErrorCode);

        // 2) 재화 조회
        var balance = await GetGoldAndGem(userId);
        if (balance.IsFailed)
            return Result<(int gold, int gem)>.Failure(balance.ErrorCode);

        var (currentGold, currentGem) = balance.Value;

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
            var updateBal = await UpdateGoldAndGem(userId, newGold, newGem);
            if (updateBal.IsFailed) return updateBal.ErrorCode;

            var insertChar = await InsertNewCharacterAsync(userId, characterCode);
            if (insertChar.IsFailed) return insertChar.ErrorCode;

            return ErrorCode.None;
        });

        if (code != ErrorCode.None)
            return Result<(int gold, int gem)>.Failure(code);

        // 6) 최신 잔액 반환
        return Result<(int gold, int gem)>.Success((newGold, newGem));
    }


    public async Task<Result> SellInventoryItemAsync(long userId, long itemId)
    {
        // 1) 아이템 조회
        var it = await GetInventoryItemAsync(userId, itemId);
        if (it.IsFailed) return Result.Failure(it.ErrorCode);
        var item = it.Value;

        // 2) 장착 여부 확인
        var eq = await IsItemEquippedAsync(itemId);
        if (eq.IsFailed) return Result.Failure(eq.ErrorCode);

        // 3) 판매가 및 현재 잔액 조회
        var (_, sellGold) = await _masterDb.GetItemSellPriceAsync(item.itemCode, item.level);

        var cur = await GetUserCurrencyAsync(userId);
        if (cur.IsFailed) return Result.Failure(cur.ErrorCode);

        var curGold = cur.Value.gold;
        var curGem  = cur.Value.gem;

        // 4) 결과 잔액 계산 (반환은 안 하지만 로직/로그에 사용)
        var newGold = curGold + sellGold;
        var newGem  = curGem;

        // 5) 트랜잭션: 인벤 삭제 → 재화 갱신
        var code = await WithTransactionAsync(async _ =>
        {
            var del = await DeleteInventoryItemAsync(userId, itemId);
            if (del.IsFailed) return del.ErrorCode;

            var up = await UpdateUserCurrencyAsync(userId, newGold, newGem);
            if (up.IsFailed) return up.ErrorCode;

            LogInfo(_logger, EventType.SellItem, "Sell Item Success",
                new { userId, itemId, beforeGold = curGold, beforeGem = curGem, newGold, newGem });

            return ErrorCode.None;
        });

        return code == ErrorCode.None ? Result.Success() : Result.Failure(code);
    }


    private async Task<Result<UserInventoryItem>> GetInventoryItemAsync(long userId, long itemId)
    {
        try
        {
            var result =  await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .FirstOrDefaultAsync<UserInventoryItem>();
            
            return result is null
                ? Result<UserInventoryItem>.Failure(ErrorCode.CannotFindInventoryItem)
                : Result<UserInventoryItem>.Success(result);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetUserInventory, 
                "Failed Get Inventory Item", new { userId, itemId });
            return Result<UserInventoryItem>.Failure(ErrorCode.FailedLoadUserData);
        }
    }

    private async Task<Result<UserInventoryRune>> GetInventoryRuneAsync(long userId, long runeId)
    {
        try
        {
            var result =  await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .FirstOrDefaultAsync<UserInventoryRune>();

            return result is null
                ? Result<UserInventoryRune>.Failure(ErrorCode.CannotFindInventoryRune)
                : Result<UserInventoryRune>.Success(result);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetUserInventory, 
                "Failed Get Inventory Rune", new { userId, runeId });
            return Result<UserInventoryRune>.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result> IsItemEquippedAsync(long itemId)
    {
        try
        {
            var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
                .Where(ITEM_ID, itemId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();
            
            return exists.HasValue 
                ? Result.Success()
                : Result.Failure(ErrorCode.AlreadyEquippedItem);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckItemEquipped, 
                "Failed Check Already Item Equipped", new { itemId });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }

    private async Task<Result> DeleteInventoryItemAsync(long userId, long itemId)
    {
        try
        {
            var affected = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .DeleteAsync();
            
            return affected == 1 
                ? Result.Success()
                : Result.Failure(ErrorCode.FailedDeleteInventoryItem);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetUserCurrency, 
                "Failed Delete Inventory Item", new { userId, e.Message, e.StackTrace });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }

    private async Task<Result<(int gold, int gem)>> GetUserCurrencyAsync(long userId)
    {
        try
        {
            var result = _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
            
            var (gold, gem) = result.Result;
            return Result<(int gold, int gem)>.Success((gold, gem));
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetUserCurrency, 
                "Failed Get User Currency", new { userId, e.Message, e.StackTrace });
            return Result<(int, int)>.Failure(ErrorCode.FailedLoadUserData);
        }
    }

    private async Task<Result> UpdateUserCurrencyAsync(long userId, int gold, int gem)
    {
        try
        {
            var affected = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .UpdateAsync(new { gold, gem });

            return affected == 1
                ? Result.Success()
                : Result.Failure(ErrorCode.FailedUpdateUserGoldAndGem);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetUserCurrency,
                "Failed Update User Currency", new { userId, e.Message, e.StackTrace });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result> CheckAlreadyHaveCharacter(long userId, long characterCode)
    {
        try
        {
            var cnt = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(USER_ID, userId)
                .Where(CHARACTER_CODE, characterCode)
                .CountAsync<long>();
            
            return cnt > 0 
                ? Result.Failure(ErrorCode.AlreadyHaveCharacter)
                : Result.Success();
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckUserHaveCharacter, 
                "Failed Check Already Have Character", new { userId, characterCode, e.Message, e.StackTrace });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result<(int,int)>> GetGoldAndGem(long userId)
    {
        try
        {
            var (gold, gem) = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
            
            return Result<(int, int)>.Success((gold, gem));
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadAllGameData , EventType.GetUserGoods, 
                "Failed Get Gold And Gem", new { userId, e.Message, e.StackTrace });
            return Result<(int, int)>.Failure(ErrorCode.FailedLoadAllGameData);
        }
    }

    private async Task<Result> UpdateGoldAndGem(long userId, int newGold, int newGem)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .UpdateAsync(new
                {
                    GOLD = newGold,
                    GEM = newGem,
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateUserGoods, 
                    "Failed Update Gold And Gem", new { userId });
                return ErrorCode.FailedUpdateGoldAndGem;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateUserGoods, 
                "Failed Update Gold And Gem", new { userId, e.Message, e.StackTrace });
            return ErrorCode.FailedUpdateGoldAndGem;
        }
    }

    private async Task<Result> InsertNewCharacterAsync(long userId, long characterCode)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .InsertAsync(new
                {
                    USER_ID = userId,
                    LEVEL = 1,
                    CHARACTER_CODE = characterCode
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.CannotInsertNewCharacter ,EventType.InsertNewCharacter, 
                    "Failed Insert New Character", new { userId, characterCode });
                return ErrorCode.CannotInsertNewCharacter;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
           LogError(_logger, ErrorCode.FailedInsertNewCharacter, EventType.InsertNewCharacter, 
               "Failed Insert New Character", new { userId, characterCode, e.Message, e.StackTrace });
           return ErrorCode.FailedInsertNewCharacter;
        }
    }
}