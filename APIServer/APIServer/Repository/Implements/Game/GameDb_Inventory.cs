using APIServer.Models.Entity.Data;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<ErrorCode> TryEquipItemAsync(long userId, long characterId, long itemId)
    {
        var code = await WithTransactionAsync(async _ =>
        {
            if (await IsCharacterExistsAsync(userId, characterId) == false)
            {
                return ErrorCode.CannotFindCharacter;
            }

            if (await IsItemExistsAsync(userId, itemId) == false)
            {
                return ErrorCode.CannotFindInventoryItem;
            }

            if (await IsItemEquippedAsync(itemId))
            {
                return ErrorCode.AlreadyEquippedItem;
            }

            if (await EquipItemAsync(characterId, itemId) == false)
            {
                return ErrorCode.FailedEquipItem;
            }
            
            return ErrorCode.None;
        });

        return code;
    }

    public async Task<ErrorCode> TryEquipRuneAsync(long userId, long characterId, long runeId)
    {
        var code = await WithTransactionAsync(async _ =>
        {
            if (await IsCharacterExistsAsync(userId, characterId) == false)
            {
                return ErrorCode.CannotFindCharacter;
            }

            if (await IsRuneExistsAsync(userId, runeId) == false)
            {
                return ErrorCode.CannotFindInventoryRune;
            }

            if (await IsRuneEquippedAsync(runeId) == false)
            {
                return ErrorCode.AlreadyEquippedRune;
            }

            if (await EquipRuneAsync(characterId, runeId) == false)
            {
                return ErrorCode.FailedEquipRune;
            }
            
            return ErrorCode.None;
        });

        return code;
    }

    public async Task<ErrorCode> TryEnhanceItemAsync(long userId, long itemId)
    {
        var item         = await GetInventoryItemAsync(userId, itemId);
        var (gold, gem)  = await GetGoldAndGem(userId);
        var enhanceData  = _masterDb.GetItemEnhanceDatas()[(item.itemCode, item.level)];

        if (await VerifyEnhanceItem(enhanceData, gold) is var verify && verify != ErrorCode.None)
        {
            return verify;
        }

        var newLevel = enhanceData.level + 1;
        var newGold  = gold - enhanceData.enhancePrice; // 원본 변경 없이 명확히 계산

        var code = await WithTransactionAsync(async _ =>
        {
            if (await UpdateItemLevel(userId, itemId, newLevel) == false)
            {
                return ErrorCode.FailedUpdateData;
            }

            if (await UpdateGoldAndGem(userId, newGold, gem) == false)
            {
                return ErrorCode.FailedUpdateGoldAndGem;
            }
            
            return ErrorCode.None;
        });

        return code;
    }

    public async Task<ErrorCode> TryEnhanceRuneAsync(long userId, long runeId)
    {
        var rune         = await GetInventoryRuneAsync(userId, runeId);
        var (gold, gem)  = await GetGoldAndGem(userId);
        var enhanceData  = _masterDb.GetRuneEnhanceDatas()[(rune.runeCode, rune.level)];

        if (await VerifyEnhanceRune(enhanceData, gold) is var verify && verify != ErrorCode.None)
        {
            return verify;
        }

        var newLevel = enhanceData.level + 1;
        var newGold  = gold - enhanceData.enhanceCount; 

        var code = await WithTransactionAsync(async _ =>
        {
            if (await UpdateRuneLevel(userId, runeId, newLevel) == false)
            {
                return ErrorCode.FailedUpdateData;
            }

            if (await UpdateGoldAndGem(userId, newGold, gem) == false)
            {
                return ErrorCode.FailedUpdateGoldAndGem;
            }

            return ErrorCode.None;
        });

        return code;
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
    
    private async Task<bool> UpdateItemLevel(long userId, long itemId, int newLevel)
    {
        var result =  await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .UpdateAsync(new
                {
                    LEVEL = newLevel,
                });

        return result == 1;
    }
    
    private async Task<bool> UpdateRuneLevel(long userId, long runeId, int newLevel)
    {
        var result = await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .UpdateAsync(new
                {
                    LEVEL = newLevel,
                });

        return result == 1;
    }
    
    private async Task<bool> EquipRuneAsync(long characterId, long runeId)
    {
        var result = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    RUNE_ID = runeId,
                });

        return result == 1;
    }

    private async Task<bool> EquipItemAsync(long characterId, long itemId)
    {
         var result =  await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    ITEM_ID = itemId,
                });

         return result == 1;
    }
    
    private async Task<bool> IsCharacterExistsAsync(long userId, long characterId)
    {
        var exists = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(CHARACTER_ID, characterId)
                .Where(USER_ID, userId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }
    
    private async Task<bool> IsRuneEquippedAsync(long runeId)
    {
        var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .Where(RUNE_ID, runeId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }

    private async Task<bool> IsItemExistsAsync(long userId, long itemId)
    {
        var isExists =  await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return isExists.HasValue;
    }
    
    private async Task<bool> IsRuneExistsAsync(long userId, long runeId)
    {
        var isExists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .Where(USER_ID, userId)
            .Where(RUNE_ID, runeId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return isExists.HasValue;
    }
}