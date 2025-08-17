using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    // 소유한 아이템인지 확인
    // 장착된 아이템인지 확인
    // 장착
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