using APIServer.Models.DTO;
using APIServer.Models.Entity;
using Dapper;
using MySqlConnector;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<long> CreateUserGameDataAndReturnUserIdAsync()
    {
        return await _queryFactory.Query(TABLE_USER_GAME_DATA).InsertGetIdAsync<long>(new
            {
                gold = 10000,
                gem = 0,
                exp = 0,
                level = 1,
                total_monster_kill_count = 0,
                total_clear_count = 0
            });
    }

    public async Task<bool> InsertCharacterAsync(long userId, UserInventoryCharacter character)
    {
        var result = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER).InsertAsync(new
            {
                character_code = character.characterCode,
                level = character.level,
                user_id = userId
            });

        return result == 1;
    }

    public async Task<bool> InsertItemAsync(long userId, UserInventoryItem item)
    {
        var result = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM).InsertAsync(new
            {
                item_code = item.itemCode,
                level = item.level,
                user_id = userId
            });

        return result == 1;
    }
    
    public async Task<bool> InsertRuneAsync(long userId, UserInventoryRune rune)
    {
        var result = await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE).InsertAsync(new
            {
                rune_code = rune.runeCode,
                level = rune.level,
                user_id = userId,
            });

        return result == 1;
    }
    
    public async Task<bool> InsertAttendanceMonthAsync(long userId)
    {
        var result = await _queryFactory.Query(TABLE_USER_ATTENDANCE_MONTH).InsertAsync(new
            {
                user_id = userId,
                last_attendance_date = 0,
                start_update_date = DateTime.MinValue,
                last_update_date = DateTime.MinValue,
            });

        return result == 1;
    }
    
    public async Task<bool> InsertAttendanceWeekAsync(long userId)
    {
        var result = await _queryFactory.Query(TABLE_USER_ATTENDANCE_WEEK).InsertAsync(new
            {
                user_id = userId,
                last_attendance_date = 0,
                start_update_date = DateTime.MinValue,
                last_update_date = DateTime.MinValue,
            });

        return result == 1;
    }
    
    public async Task<bool> InsertQuestAsync(long userId, long questCode, DateTime expireDate)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_INPROGRESS).InsertAsync(new
            {
                user_id = userId,
                quest_code = questCode,
                expire_date = expireDate,
                progress = 0
            });

        return result == 1;
    }

    public async Task<bool> DeleteGameDataByUserIdAsync(long userId)
    {
        try
        {
            await _queryFactory.Query(TABLE_USER_GAME_DATA).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_ATTENDANCE_MONTH).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_ATTENDANCE_WEEK).Where(USER_ID, userId).DeleteAsync();
            await _queryFactory.Query(TABLE_USER_QUEST_INPROGRESS).Where(USER_ID, userId).DeleteAsync();
            
            LogInfo(_logger, EventType.RollBackDefaultData, "Success Rollback Default Data", new { userId });
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedRollbackDefaultData, EventType.RollBackDefaultData, 
                "Delete GameData By UserId Async Failed", new
                {
                    e.Message,
                    e.StackTrace
                });
            return false;
        }

        return true;
    }

    public Task<UserGameData> GetUserDataByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserDataAsync(UserGameData data)
    {
        throw new NotImplementedException();
    }
}