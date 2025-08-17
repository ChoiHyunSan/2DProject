using APIServer.Models.Entity.Data;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<ErrorCode> TryEquipItem(long userId, long characterId, long itemId)
    {
        return await WithTransactionAsync(async q =>
        {
            if (await IsCharacterExistsAsync(q, userId, characterId) == false)
            {
                LogError(_logger, ErrorCode.CannotFindCharacter, EventType.EquipItem, "Cannot Find Character", new {userId, characterId});
                return ErrorCode.CannotFindCharacter;
            }
            
            if (await IsItemExistsAsync(q, userId, itemId) == false)
            {
                LogError(_logger, ErrorCode.CannotFindInventoryItem, EventType.EquipItem, "Cannot Find Inventory Item", new{ userId, itemId});
                return ErrorCode.CannotFindInventoryItem;
            }

            if (await IsItemEquippedAsync(q, itemId))
            {
                LogError(_logger, ErrorCode.AlreadyEquippedItem, EventType.EquipItem, "Already Equipped Item", new{ userId, itemId});
                return ErrorCode.AlreadyEquippedItem;
            }

            return await EquipItemAsync(q, characterId, itemId);
        });
    }

    public async Task<ErrorCode> TryEquipRune(long userId, long characterId, long runeId)
    {
        return await WithTransactionAsync(async q =>
        {
            if (await IsCharacterExistsAsync(q, userId, characterId) == false)
            {
                LogError(_logger, ErrorCode.CannotFindCharacter, EventType.EquipItem, "Cannot Find Character", new {userId, characterId});
                return ErrorCode.CannotFindCharacter;
            }
            
            if (await IsRuneExistsAsync(q, userId, runeId) == false)
            {
                LogError(_logger, ErrorCode.CannotFindInventoryRune, EventType.EquipRune, "Cannot Find Inventory Rune", new { userId, runeId });
                return ErrorCode.CannotFindInventoryRune;
            }

            if (await IsRuneEquippedAsync(q, runeId))
            {
                LogError(_logger, ErrorCode.AlreadyEquippedRune, EventType.EquipRune, "Already Equipped Rune", new { userId, runeId });
                return ErrorCode.AlreadyEquippedRune;
            }

            return await EquipRuneAsync(q, characterId, runeId);
        });
    }

    public async Task<ErrorCode> TryEnhanceItem(long userId, long itemId)
    {
        return await WithTransactionAsync(async q =>
        {
            var item = await GetInventoryItemAsync(q, userId, itemId);
            if (item is null)
            {
                LogError(_logger, ErrorCode.CannotFindInventoryItem, EventType.EquipItem, "Cannot Find Inventory Item", new{ userId, itemId});
                return ErrorCode.CannotFindInventoryItem;
            }

            var enhanceData = await GetItemEnhanceData(item.itemCode, item.level);
            if (enhanceData is null)
            {
                LogError(_logger, ErrorCode.FailedGetItemEnhanceData, EventType.EnhanceItem, "Enhance Data", new{ userId, itemId});
                return ErrorCode.FailedGetItemEnhanceData;
            }

            var (errorCode, gold, gem) = await GetGoldAndGem(userId);
            if (errorCode != ErrorCode.None)
            {
                LogError(_logger, ErrorCode.FailedGetUserGoldAndGem, EventType.EnhanceItem, "Enhance Data", new{ userId});   
                return errorCode;
            }

            var verifyResult = await VerifyEnhanceItem(enhanceData, gold);
            if (verifyResult != ErrorCode.None)
            {
                LogError(_logger, verifyResult, EventType.EnhanceItem, "Cannot Enhance Item", new { userId, itemId });
                return verifyResult;
            }
            
            var newLevel = enhanceData.level + 1;
            var newGold = gold -= enhanceData.enhancePrice;

            errorCode = await UpdateItemLevel(q, userId, itemId, newLevel);
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }
            
            errorCode = await UpdateGoldAndGem(userId, newGold, gem);
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }
            
            return ErrorCode.None;
        });
    }

    public async Task<ErrorCode> TryEnhanceRune(long userId, long runeId)
    {
        return await WithTransactionAsync(async q =>
        {
            var rune = await GetInventoryRuneAsync(q, userId, runeId);
            if (rune is null)
            {
                LogError(_logger, ErrorCode.CannotFindInventoryRune, EventType.EquipRune, "Cannot Find Inventory Rune", new { userId, runeId });
                return ErrorCode.CannotFindInventoryRune;
            }

            var enhanceData = await GetRuneEnhanceData(rune.runeCode, rune.level);
            if (enhanceData is null)
            {
                LogError(_logger, ErrorCode.FailedGetRuneEnhanceData, EventType.EnhanceRune, "Enhance Data", new{ userId, runeId });
                return ErrorCode.FailedGetRuneEnhanceData;
            }

            var (errorCode, gold, gem) = await GetGoldAndGem(userId);
            if (errorCode != ErrorCode.None)
            {
                LogError(_logger, ErrorCode.FailedGetUserGoldAndGem, EventType.EnhanceItem, "Enhance Data", new{ userId});   
                return errorCode;
            }
            
            var verifyResult = await VerifyEnhanceRune(enhanceData, gold);
            if (verifyResult != ErrorCode.None)
            {
                LogError(_logger, verifyResult, EventType.EnhanceItem, "Cannot Enhance Item", new { userId, runeId });
                return verifyResult;
            }
            
            var newLevel = enhanceData.level + 1;
            var newGold = gold -= enhanceData.enhanceCount;

            errorCode = await UpdateRuneLevel(q, userId, runeId, newLevel);
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }
            
            errorCode = await UpdateGoldAndGem(userId, newGold, gem);
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }
            
            return ErrorCode.None;
        });
    }
    
    private async Task<ErrorCode> VerifyEnhanceItem(ItemEnhanceData enhanceData, int gold)
    {
        if (enhanceData.level >= 3)
        {
            return ErrorCode.AlreadyMaximumLevelItem;
        }

        if (enhanceData.enhancePrice > gold)
        {
            return ErrorCode.GoldShortage;
        }

        return ErrorCode.None;
    }

    private async Task<ErrorCode> VerifyEnhanceRune(RuneEnhanceData enhanceData, int gold)
    {
        if (enhanceData.level >= 3)
        {
            return ErrorCode.AlreadyMaximumLevelRune;
        }

        if (enhanceData.enhanceCount > gold)
        {
            return ErrorCode.GoldShortage;
        }

        return ErrorCode.None;
    }
    
    private async Task<ErrorCode> UpdateItemLevel(QueryFactory q, long userId, long itemId, int newLevel)
    {
        var result = await q.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .UpdateAsync(new
            {
                LEVEL = newLevel,
            });

        if (result == 0)
        {
            LogError(_logger, ErrorCode.FailedUpdateItemLevel, EventType.UpdateItemLevel, "Failed Update Item Level", new
            {
                userId, itemId, newLevel
            });
            return ErrorCode.FailedUpdateItemLevel;
        }

        return ErrorCode.None;
    }
    
    private async Task<ErrorCode> UpdateRuneLevel(QueryFactory q, long userId, long runeId, int newLevel)
    {
        var result = await q.Query(TABLE_USER_INVENTORY_RUNE)
            .Where(USER_ID, userId)
            .Where(RUNE_ID, runeId)
            .UpdateAsync(new
            {
                LEVEL = newLevel,
            });

        if (result == 0)
        {
            LogError(_logger, ErrorCode.FailedUpdateRuneLevel, EventType.UpdateRuneLevel, "Failed Update Rune Level", new
            {
                userId, runeId, newLevel
            });
            return ErrorCode.FailedUpdateRuneLevel;
        }

        return ErrorCode.None;
    }
    
    private async Task<ItemEnhanceData?> GetItemEnhanceData(long itemCode, int level)
    {
        var (errorCode, itemEnhanceData) = await _masterDb.GetItemEnhanceData(itemCode, level);
        if (errorCode != ErrorCode.None)
        {
            return null;
        }

        return itemEnhanceData;
    }

    private async Task<RuneEnhanceData?> GetRuneEnhanceData(long runeCode, int level)
    {
        var (errorCode, runeEnhanceData) = await _masterDb.GetRuneEnhanceData(runeCode, level);
        if (errorCode != ErrorCode.None)
        {
            return null;
        }

        return runeEnhanceData;
    }
    
    private async Task<ErrorCode> EquipRuneAsync(QueryFactory q, long characterId, long runeId)
    {
        var result = await q.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .InsertAsync(new
            {
                CHARACTER_ID = characterId,
                RUNE_ID = runeId,
            });

        if (result != 1)
        {
            LogError(_logger, ErrorCode.FailedInsertCharacterRune, EventType.EquipRune, "Failed Equip Rune", new { characterId, runeId });;
            return ErrorCode.FailedInsertCharacterRune;
        }

        return ErrorCode.None;
    }

    private async Task<ErrorCode> EquipItemAsync(QueryFactory q, long characterId, long itemId)
    {
        var result = await q.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
            .InsertAsync(new
            {
                CHARACTER_ID = characterId,
                ITEM_ID = itemId,
            });

        if (result != 1)
        {
            LogError(_logger, ErrorCode.FailedInsertCharacterItem, EventType.EquipRune, "Failed Equip Rune", new { characterId, itemId });
            return ErrorCode.FailedInsertCharacterItem;
        }

        return ErrorCode.None;
    }
    
    private async Task<bool> IsCharacterExistsAsync(QueryFactory q, long userId, long characterId)
    {
        var exists = await q.Query(TABLE_USER_INVENTORY_CHARACTER)
            .Where(CHARACTER_ID, characterId)
            .Where(USER_ID, userId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();
        
        return exists.HasValue;
    }
    
    private async Task<bool> IsRuneEquippedAsync(QueryFactory q, long runeId)
    {
        var exists = await q.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .Where(RUNE_ID, runeId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();
        return exists.HasValue;
    }

    private async Task<bool> IsItemExistsAsync(QueryFactory q, long userId, long itemId)
    {
        var isExists =  await q.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return isExists.HasValue;
    }
    
    private async Task<bool> IsRuneExistsAsync(QueryFactory q, long userId, long runeId)
    {
        var isExists =  await q.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .Where(USER_ID, userId)
            .Where(RUNE_ID, runeId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return isExists.HasValue;
    }
}