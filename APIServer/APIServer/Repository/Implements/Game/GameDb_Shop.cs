using APIServer.Models.Entity;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<UserInventoryItem> GetInventoryItemAsync(long userId, long itemId)
    {
        return await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
                .Where(USER_ID, userId)
                .Where(ITEM_ID, itemId)
                .FirstOrDefaultAsync<UserInventoryItem>();
    }

    public async Task<UserInventoryRune> GetInventoryRuneAsync(long userId, long runeId)
    {
        return await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
                .Where(USER_ID, userId)
                .Where(RUNE_ID, runeId)
                .FirstOrDefaultAsync<UserInventoryRune>();
    }
    
    public async Task<bool> IsItemEquippedAsync(long itemId)
    {
        var exists = await _queryFactory.Query(TABLE_CHARACTER_EQUIPMENT_ITEM)
            .Where(ITEM_ID, itemId)
            .SelectRaw("1")
            .Limit(1)
            .FirstOrDefaultAsync<int?>();

        return exists.HasValue;
    }

    public async Task<bool> DeleteInventoryItemAsync(long userId, long itemId)
    {
        var affected = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
            .Where(USER_ID, userId)
            .Where(ITEM_ID, itemId)
            .DeleteAsync();

        return affected == 1;
    }

    public async Task<(int gold, int gem)> GetUserCurrencyAsync(long userId)
    {
        return await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
    }

    public async Task<bool> UpdateUserCurrencyAsync(long userId, int gold, int gem)
    {
        var affected = await _queryFactory.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new { gold, gem });

        return affected == 1;
    }
    
    public async Task<bool> CheckAlreadyHaveCharacterAsync(long userId, long characterCode)
    {
        var cnt = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
            .Where(USER_ID, userId)
            .Where(CHARACTER_CODE, characterCode)
            .CountAsync<long>();

        return cnt > 0;
    }

    public async Task<bool> InsertNewCharacterAsync(long userId, long characterCode)
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

    public async Task<bool> UpdateUserGemAsync(long userId, int price)
    {
        var result = await _queryFactory.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                GEM = price
            });

        return result == 1;
    }
}