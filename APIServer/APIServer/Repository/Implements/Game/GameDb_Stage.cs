using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using SqlKata;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<List<UserClearStage>> GetClearStageListAsync(long userId)
    {
        var result =  await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .GetAsync<UserClearStage>();

        return result.ToList();
    }
    
    public async Task<bool> UpdateClearStageAsync(long userId, long stageCode)
    {
        try
        {
            var clearStage = await FindClearStageAsync(userId, stageCode);
            bool update = await UpdateStageAsync(clearStage);
            if (!update)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedUpdateData, EventType.GetClearStage
            , "Failed Update Clear Stage", new { userId, stageCode, ex.Message });

            return false;
        }
    }

    public Task<bool> RewardClearStageAsync(InStageInfo stageInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<UserClearStage> FindClearStageAsync(long userId, long stageCode)
    {
        return await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .Where(USER_ID, userId)
            .Where(STAGE_CODE, stageCode)
            .FirstOrDefaultAsync<UserClearStage>();
    }
    
    public async Task<bool> InsertClearStageAsync(long userId, long stageCode)
    {
        var inserted = await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .InsertAsync(new
            {
                user_id          = userId,
                stage_code       = stageCode,
                clear_count      = 1,
                first_clear_date = DateTime.UtcNow,
                last_clear_date  = DateTime.UtcNow
            });

        return inserted == 1;
    }
    
    public async Task<bool> UpdateStageAsync(UserClearStage current)
    {
        var updated = await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .Where(USER_ID, current.user_id)
            .Where(STAGE_CODE, current.stage_code)
            .UpdateAsync(new
            {
                clear_count     = current.clear_count + 1,
                last_clear_date = DateTime.UtcNow
            });

        return updated == 1;
    }

    public async Task<bool> UpdateUserGoldAsync(long userId, int newGold)
    {
        var updated = await _queryFactory.Query(TABLE_USER_GAME_DATA)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                GOLD = newGold
            });

        return updated == 1;
    }

    public async Task<bool> InsertDropItems(long userId, List<StageRewardItem> dropItems)
    {
        var updated = await _queryFactory.Query(TABLE_USER_INVENTORY_ITEM)
            .InsertAsync(dropItems.Select(di => new
            {
                ITEM_CODE = di.item_code,
                USER_ID = userId,
                LEVEL = 1
            }));
        
        return updated == dropItems.Count;
    }

    public async Task<bool> InsertDropRunes(long userId, List<StageRewardRune> dropRunes)
    {
        var updated = await _queryFactory.Query(TABLE_USER_INVENTORY_RUNE)
            .InsertAsync(dropRunes.Select(di => new
            {
                RUNE_CODE = di.rune_code,
                USER_ID = userId,
                LEVEL = 1
            }));
        
        return updated == dropRunes.Count;
    }
}