using APIServer.Models.Entity;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<Result<(int, int)>> PurchaseCharacterAsync(long userId, long characterCode, int goldPrice, int gemPrice)
    {
        var checkCharacter = await CheckAlreadyHaveCharacter(userId, characterCode);
        if (checkCharacter.IsFailed) return Result<(int,int)>.Failure(checkCharacter.ErrorCode);
        
        var getGoldAndGem = await GetGoldAndGem(userId);
        if (getGoldAndGem.IsFailed) return Result<(int,int)>.Failure(getGoldAndGem.ErrorCode);
        
        var (currentGold, currentGem) = getGoldAndGem.Value;
        if (goldPrice > currentGold || gemPrice > currentGem)
        {
            return Result<(int,int)>.Failure(ErrorCode.CannotPurchaseCharacter);
        }

        var newGold = currentGold - goldPrice;
        var newGem = currentGem - gemPrice;
        
        var txCode = await WithTransactionAsync(async q =>
        {
            var updated = await UpdateGoldAndGem(userId, newGold, newGem);
            if (updated.IsFailed) return updated.ErrorCode;

            var insertNew = await InsertNewCharacterAsync(userId, characterCode);
            if (insertNew.IsFailed) return insertNew.ErrorCode;

            return ErrorCode.None;
        });

        if (txCode != ErrorCode.None)
        {
            return Result<(int,int)>.Failure(txCode);    
        }

        return Result<(int,int)>.Success((newGold, newGem));
    }

    public async Task<Result> SellInventoryItemAsync(long userId, long itemId)
    {
        var txCode = await WithTransactionAsync(async q =>
        {
            // 1) 아이템 조회
            var getItem = await GetInventoryItemAsync(userId, itemId);
            if (getItem.IsFailed) return getItem.ErrorCode;
            var item = getItem.Value;
            
            // 2) 장착 여부 확인
            var equipped = await IsItemEquippedAsync(itemId);
            if (equipped.IsFailed) return equipped.ErrorCode;

            // 3) 인벤토리에서 삭제
            var deleted = await DeleteInventoryItemAsync(userId, itemId);
            if (deleted.IsFailed) return deleted.ErrorCode;

            // 4) 현재 재화 조회
            var currency = await GetUserCurrencyAsync(userId);
            if (currency.IsFailed) return currency.ErrorCode;

            // 5) 마스터에서 판매가 조회(골드 증가)
            var (_, gold) = await _masterDb.GetItemSellPriceAsync(item.itemCode, item.level);

            var newGold = currency.Value.gold + gold;
            var newGem  = currency.Value.gem;

            // 6) 재화 업데이트
            var updated = await UpdateUserCurrencyAsync(userId, newGold, newGem);
            if (updated.IsFailed) return updated.ErrorCode;

            LogInfo(_logger, EventType.SellItem, "Sell Item Success", new { userId, itemId, newGold, newGem });
            
            return ErrorCode.None;
        });

        return txCode == ErrorCode.None ? Result.Success() : Result.Failure(txCode);
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