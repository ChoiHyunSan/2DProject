using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<bool> UpdateItemLevelAsync(long userId, long itemId, int newLevel)
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

    public async Task<UserInventoryCharacter> GetInventoryCharacterAsync(long userId, long characterId)
    {
        return await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
            .Where(USER_ID, userId)
            .Where(CHARACTER_ID, characterId)
            .FirstOrDefaultAsync<UserInventoryCharacter>();
    }

    public async Task<bool> UpdateCharacterLevelAsync(long userId, long characterId, int newLevel)
    {
        var result =  await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
            .Where(USER_ID, userId)
            .Where(CHARACTER_ID, characterId)
            .UpdateAsync(new
            {
                LEVEL = newLevel,
            });

        return result == 1;
    }

    public async Task<bool> UpdateRuneLevelAsync(long userId, long runeId, int newLevel)
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
    
    public async Task<bool> EquipRuneAsync(long characterId, long runeId)
    {
        var result = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    RUNE_ID = runeId,
                });

        return result == 1;
    }

    public async Task<bool> EquipItemAsync(long characterId, long itemId)
    {
         var result =  await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
                .InsertAsync(new
                {
                    CHARACTER_ID = characterId,
                    ITEM_ID = itemId,
                });

         return result == 1;
    }
    
    public async Task<bool> IsCharacterExistsAsync(long userId, long characterId)
    {
        var exists = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(CHARACTER_ID, characterId)
                .Where(USER_ID, userId)
                .SelectRaw("1")
                .Limit(1)
                .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }
    
    public async Task<bool> IsRuneEquippedAsync(long runeId)
    {
        var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_RUNE)
            .Where(RUNE_ID, runeId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }

    public async Task<bool> IsItemExistsAsync(long userId, long itemId)
    {
        var isExists =  await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return isExists.HasValue;
    }
    
    public async Task<bool> IsRuneExistsAsync(long userId, long runeId)
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