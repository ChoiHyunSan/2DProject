using APIServer.Models.Entity;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<(ErrorCode, int , int)> PurchaseCharacter(long userId, long characterCode, int goldPrice, int gemPrice)
    {
        var (checkErrorCode, isAlreadyHave) = await CheckAlreadyHaveCharacter(userId, characterCode);
        if (checkErrorCode != ErrorCode.None || isAlreadyHave)
        {
            return (checkErrorCode, 0, 0);
        }
        
        var (errorCode, currentGold, currentGem) = await GetGoldAndGem(userId);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, currentGold, currentGem);
        }

        if (goldPrice > currentGold || gemPrice > currentGem)
        {
            return (ErrorCode.CannotPurchaseCharacter, currentGold, currentGem);
        }

        var newGold = currentGold - goldPrice;
        var newGem = currentGem - gemPrice;
        
        var txCode = await WithTransactionAsync(async q =>
        {
            var e1 = await UpdateGoldAndGem(userId, newGold, newGem);
            if (e1 != ErrorCode.None)
                return e1;

            var e2 = await InsertNewCharacter(userId, characterCode);
            if (e2 != ErrorCode.None)
                return e2;

            return ErrorCode.None;
        });

        if (txCode != ErrorCode.None)
        {
            return (txCode, currentGold, currentGem);    
        }

        return (ErrorCode.None, newGold, newGem);
    }

    public async Task<ErrorCode> SellInventoryItem(long userId, long itemId)
    {
        var txCode = await WithTransactionAsync(async q =>
        {
            // 1) 아이템 조회
            var item = await GetInventoryItemAsync(q, userId, itemId);
            if (item == null)
                return ErrorCode.CannotFindInventoryItem;

            // 2) 장착 여부 확인
            var equipped = await IsItemEquippedAsync(q, itemId);
            if (equipped)
                return ErrorCode.CannotSellEquipmentItem;

            // 3) 인벤토리에서 삭제
            var deleted = await DeleteInventoryItemAsync(q, userId, itemId);
            if (!deleted)
                return ErrorCode.FailedDeleteInventoryItem;

            // 4) 현재 재화 조회
            var currency = await GetUserCurrencyAsync(q, userId);
            if (currency == null)
                return ErrorCode.FailedGetUserGoldAndGem;

            // 5) 마스터에서 판매가 조회(골드 증가)
            var (_, gold) = await _masterDb.GetItemSellPriceAsync(item.itemCode, item.level);

            var newGold = currency.Value.gold + gold;
            var newGem  = currency.Value.gem;

            // 6) 재화 업데이트
            var updated = await UpdateUserCurrencyAsync(q, userId, newGold, newGem);
            if (!updated)
                return ErrorCode.FailedUpdateUserGoldAndGem;

            LogInfo(_logger, EventType.SellItem, "Sell Item Success", new
            {
                userId, itemId,
                newGold, newGem
            });
            
            return ErrorCode.None;
        });

        return txCode;
    }

    private Task<UserInventoryItem?> GetInventoryItemAsync(QueryFactory q, long userId, long itemId)
    {
        return q.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .FirstOrDefaultAsync<UserInventoryItem>();
    }

    private async Task<bool> IsItemEquippedAsync(QueryFactory q, long itemId)
    {
        var exists = await q.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();
        return exists.HasValue;
    }

    private async Task<bool> DeleteInventoryItemAsync(QueryFactory q, long userId, long itemId)
    {
        var affected = await q.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .DeleteAsync();
        return affected == 1;
    }

    private Task<(int gold, int gem)?> GetUserCurrencyAsync(QueryFactory q, long userId)
    {
        return q.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .Select(GOLD, GEM)
            .FirstOrDefaultAsync<(int gold, int gem)?>();
    }

    private async Task<bool> UpdateUserCurrencyAsync(QueryFactory q, long userId, int gold, int gem)
    {
        var affected = await q.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new { gold, gem });
        return affected == 1;
    }
    
    private async Task<(ErrorCode, bool)> CheckAlreadyHaveCharacter(long userId, long characterCode)
    {
        try
        {
            LogInfo(_logger, EventType.CheckAlreadyHaveCharacter, "Check Already Have Character", new { userId, characterCode });

            var cnt = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(USER_ID, userId)
                .Where(CHARACTER_CODE, characterCode)
                .CountAsync<long>();
            
            return cnt > 0 
                ? (ErrorCode.AlreadyHaveCharacter, true) 
                : (ErrorCode.None, false);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckAlreadyHaveCharacter, "Failed Check Already Have Character", new
            {
                userId, characterCode,
                e.Message,
                e.StackTrace
            });
            return (ErrorCode.FailedLoadUserData, false);
        }
    }
    
    private async Task<(ErrorCode, int, int)> GetGoldAndGem(long userId)
    {
        try
        {
            LogInfo(_logger, EventType.GetGoldAndGem, "Get Gold And Gem", new { userId });
            
            var (gold, gem) = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
            
            return (ErrorCode.None, gold, gem);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadAllGameData , EventType.GetGoldAndGem, "Failed Get Gold And Gem", new
            {
                userId,
                e.Message,
                e.StackTrace
            });
            return (ErrorCode.FailedLoadAllGameData, 0, 0);
        }
    }

    private async Task<ErrorCode> UpdateGoldAndGem(long userId, int newGold, int newGem)
    {
        try
        {
            LogInfo(_logger, EventType.UpdateGoldAndGem, "Update Gold And Gem", new { userId, newGold, newGem });

            var result = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .UpdateAsync(new
                {
                    GOLD = newGold,
                    GEM = newGem,
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateGoldAndGem, "Failed Update Gold And Gem", new
                {
                    userId
                });
                return ErrorCode.FailedUpdateGoldAndGem;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateGoldAndGem, "Failed Update Gold And Gem", new
            {
                userId, 
                e.Message,
                e.StackTrace
            });
            return ErrorCode.FailedUpdateGoldAndGem;
        }
    }

    private async Task<ErrorCode> InsertNewCharacter(long userId, long characterCode)
    {
        try
        {
            LogInfo(_logger, EventType.InsertNewCharacter, "Insert New Character", new { userId, characterCode });

            var result = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .InsertAsync(new
                {
                    USER_ID = userId,
                    LEVEL = 1,
                    CHARACTER_CODE = characterCode
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.CannotInsertNewCharacter ,EventType.InsertNewCharacter, "Failed Insert New Character", new
                {
                    userId, 
                    characterCode
                });
                return ErrorCode.CannotInsertNewCharacter;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
           LogError(_logger, ErrorCode.FailedInsertNewCharacter, EventType.InsertNewCharacter, "Failed Insert New Character", new
           {
               userId, characterCode,
               e.Message,
               e.StackTrace
           });
           return ErrorCode.FailedInsertNewCharacter;
        }
    }
}