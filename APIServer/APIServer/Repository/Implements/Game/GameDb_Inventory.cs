using APIServer.Models.Entity.Data;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<Result> TryEquipItemAsync(long userId, long characterId, long itemId)
    {
        var code = await WithTransactionAsync(async _ =>
        {
            var checkCharacter = await IsCharacterExistsAsync(userId, characterId);
            if (checkCharacter.IsFailed) return checkCharacter.ErrorCode;

            var checkItem = await IsItemExistsAsync(userId, itemId);
            if(checkItem.IsFailed) return  checkItem.ErrorCode;

            var checkItemEquip = await IsItemEquippedAsync(itemId);
            if(checkItemEquip.IsFailed)  return checkItemEquip.ErrorCode;

            var equipItem = await EquipItemAsync(characterId, itemId);
            return equipItem.ErrorCode;
        });

        return code == ErrorCode.None ? Result.Success() : Result.Failure(code);
    }

    public async Task<Result> TryEquipRuneAsync(long userId, long characterId, long runeId)
    {
        var code = await WithTransactionAsync(async q =>
        {
            var checkCharacter = await IsCharacterExistsAsync(userId, characterId);
            if(checkCharacter.IsFailed) return checkCharacter.ErrorCode;

            var checkRune = await IsRuneExistsAsync(userId, runeId);   
            if(checkRune.IsFailed) return checkRune.ErrorCode;

            var  checkRuneEquip = await IsRuneEquippedAsync(runeId);
            if(checkRuneEquip.IsFailed) return  checkRuneEquip.ErrorCode;
            
            var equipRune = await EquipRuneAsync(characterId, runeId);
            return  equipRune.ErrorCode;
        });

        return code == ErrorCode.None ? Result.Success() : Result.Failure(code);
    }

    public async Task<Result> TryEnhanceItemAsync(long userId, long itemId)
    {
        var getItem = await GetInventoryItemAsync(userId, itemId);
        if (getItem.IsFailed) return getItem.ErrorCode;
        var item = getItem.Value;

        var getEnhance = await GetItemEnhanceData(item.itemCode, item.level);
        if (getEnhance.IsFailed) return getEnhance.ErrorCode;
        var enhance = getEnhance.Value;

        var getBal = await GetGoldAndGem(userId);
        if (getBal.IsFailed) return getBal.ErrorCode;
        var (gold, gem) = getBal.Value;

        var verify = await VerifyEnhanceItem(enhance, gold);
        if (verify.IsFailed) return verify.ErrorCode;

        var newLevel = enhance.level + 1;
        var newGold  = gold - enhance.enhancePrice; // 원본 변경 없이 명확히 계산

        var code = await WithTransactionAsync(async _ =>
        {
            var upItem = await UpdateItemLevel(userId, itemId, newLevel);
            if (upItem.IsFailed) return upItem.ErrorCode;

            var upBal = await UpdateGoldAndGem(userId, newGold, gem);
            if (upBal.IsFailed) return upBal.ErrorCode;

            return ErrorCode.None;
        });

        return code;
    }

    public async Task<Result> TryEnhanceRuneAsync(long userId, long runeId)
    {
        var getRune = await GetInventoryRuneAsync(userId, runeId);
        if (getRune.IsFailed) return getRune.ErrorCode;
        var rune = getRune.Value;

        var getEnhance = await GetRuneEnhanceData(rune.runeCode, rune.level);
        if (getEnhance.IsFailed) return getEnhance.ErrorCode;
        var enhance = getEnhance.Value;

        var getBal = await GetGoldAndGem(userId);
        if (getBal.IsFailed) return getBal.ErrorCode;
        var (gold, gem) = getBal.Value;

        var verify = await VerifyEnhanceRune(enhance, gold);
        if (verify.IsFailed) return verify.ErrorCode;

        var newLevel = enhance.level + 1;
        var newGold  = gold - enhance.enhanceCount; 

        var code = await WithTransactionAsync(async _ =>
        {
            var upRune = await UpdateRuneLevel(userId, runeId, newLevel);
            if (upRune.IsFailed) return upRune.ErrorCode;

            var upBal = await UpdateGoldAndGem(userId, newGold, gem);
            if (upBal.IsFailed) return upBal.ErrorCode;

            return ErrorCode.None;
        });

        return code;
    }
    
    private async Task<Result> VerifyEnhanceItem(ItemEnhanceData enhanceData, int gold)
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

    private async Task<Result> VerifyEnhanceRune(RuneEnhanceData enhanceData, int gold)
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
    
    private async Task<Result> UpdateItemLevel(long userId, long itemId, int newLevel)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
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
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedUpdateItemLevel, EventType.UpdateItemLevel, 
                "Failed Update Rune Level", new { userId, itemId, newLevel });
            return ErrorCode.FailedUpdateItemLevel;
        }
    }
    
    private async Task<Result> UpdateRuneLevel(long userId, long runeId, int newLevel)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .UpdateAsync(new
                {
                    LEVEL = newLevel,
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.FailedUpdateRuneLevel, EventType.UpdateRuneLevel, 
                    "Failed Update Rune Level", new { userId, runeId, newLevel });
                return ErrorCode.FailedUpdateRuneLevel;
            }

            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedUpdateRuneLevel, EventType.EquipRune, 
                "Failed Update Rune Level", new { userId, runeId });
            return Result.Failure(ErrorCode.FailedUpdateRuneLevel);   
        }
    }
    
    private async Task<Result<ItemEnhanceData>> GetItemEnhanceData(long itemCode, int level)
    {
        var result = await _masterDb.GetItemEnhanceDataAsync(itemCode, level);
        if (result.IsFailed)
        {
            return Result<ItemEnhanceData>.Failure(result.ErrorCode);
        }

        return Result<ItemEnhanceData>.Success(result.Value);
    }

    private async Task<Result<RuneEnhanceData>> GetRuneEnhanceData(long runeCode, int level)
    {
        var result = await _masterDb.GetRuneEnhanceDataAsync(runeCode, level);
        if (result.IsFailed)
        {
            return Result<RuneEnhanceData>.Failure(result.ErrorCode);
        }

        return Result<RuneEnhanceData>.Success(result.Value);
    }
    
    private async Task<Result> EquipRuneAsync(long characterId, long runeId)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    RUNE_ID = runeId,
                });

            if (result != 1)
            {
                LogError(_logger, ErrorCode.FailedInsertCharacterRune, EventType.EquipRune, "Failed Equip Rune", new { characterId, runeId });;
                return Result.Failure(ErrorCode.FailedInsertCharacterRune);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedInsertCharacterRune, EventType.EquipRune, 
                "Failed Equip Rune", new { characterId, runeId });
            return Result.Failure(ErrorCode.FailedInsertCharacterRune);   
        }
    }

    private async Task<Result> EquipItemAsync(long characterId, long itemId)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    ITEM_ID = itemId,
                });

            if (result != 1)
            {
                LogError(_logger, ErrorCode.FailedInsertCharacterItem, EventType.EquipItem, 
                    "Failed Equip Item", new { characterId, itemId });
                return Result.Failure(ErrorCode.FailedInsertCharacterItem);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.EquipItem, 
                "Failed Check Character Exists", new { characterId });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result> IsCharacterExistsAsync(long userId, long characterId)
    {
        try
        {
            var exists = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(CHARACTER_ID, characterId)
                .Where(USER_ID, userId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();

            return exists.HasValue
                ? Result.Success()
                : Result.Failure(ErrorCode.CannotFindCharacter);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckUserHaveCharacter, 
                "Failed Check Character Exists", new { characterId });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result> IsRuneEquippedAsync(long runeId)
    {
        try
        {
            var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
                .Where(RUNE_ID, runeId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();   
            
            return exists.HasValue 
                ? Result.Success()
                : Result.Failure(ErrorCode.AlreadyEquippedRune);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckRuneEquipped, 
                "Failed Check Already Rune Equipped", new { runeId });
            return Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }

    private async Task<Result> IsItemExistsAsync(long userId, long itemId)
    {
        try
        {
            var isExists =  await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();
            
            return isExists.HasValue
                ? Result.Success()
                : Result.Failure(ErrorCode.CannotFindInventoryItem);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckItemExists, 
                "Failed Load UserData", new { userId, itemId });
            return  Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    private async Task<Result> IsRuneExistsAsync(long userId, long runeId)
    {
        try
        {
            var isExists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();

            return isExists.HasValue
                ? Result.Success()
                : Result.Failure(ErrorCode.CannotFindInventoryRune);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckRuneExists, 
                "Failed Load UserData", new { userId, runeId });
            return  Result.Failure(ErrorCode.FailedLoadUserData);
        }
    }
}