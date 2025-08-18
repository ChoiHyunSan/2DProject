using APIServer.Models.Entity;
using APIServer.Models.Redis;
using SqlKata;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<Result<List<UserClearStage>>> GetClearStageList(long userId)
    {
        try
        {
            var result = await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
                .GetAsync<UserClearStage>();
            
            return Result<List<UserClearStage>>.Success(result.ToList());
        }
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedLoadUserData, EventType.GetClearStage
                , "Failed Get Clear Stage List", new { userId, e.Message, e.StackTrace});
            return Result<List<UserClearStage>>.Failure(ErrorCode.FailedLoadUserData);
        }
    }
    
    public async Task<Result> UpdateClearStageAsync(long userId, long stageCode)
    {
        try
        {
            var existing = await FindClearStageAsync(userId, stageCode);
            if (existing.IsFailed) return Result.Failure(existing.ErrorCode);

            var clearStage = existing.Value;
            if (clearStage is null)
            {
                return await InsertClearStageAsync(userId, stageCode);
            }
            
            return await UpdateStageAsync(clearStage);
        }
        catch (Exception ex)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedUpdateData, EventType.GetClearStage
            , "Failed Update Clear Stage", new { userId, stageCode, ex.Message });
            
            return ErrorCode.FailedInsertData;
        }
    }

    public Task<Result> RewardClearStage(InStageInfo stageInfo)
    {
        throw new NotImplementedException();
    }

    private async Task<Result<UserClearStage>> FindClearStageAsync(long userId, long stageCode)
    {
        var result =  await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .Where(USER_ID, userId)
            .Where(STAGE_CODE, stageCode)
            .FirstOrDefaultAsync<UserClearStage>();
        
        return Result<UserClearStage>.Success(result);
    }
    
    private async Task<Result> InsertClearStageAsync(long userId, long stageCode)
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

        return inserted == 1 ? ErrorCode.None : ErrorCode.FailedInsertData;
    }
    
    private async Task<Result> UpdateStageAsync(UserClearStage current)
    {
        var updated = await _queryFactory.Query(TABLE_USER_CLEAR_STAGE)
            .Where(USER_ID, current.userId)
            .Where(STAGE_CODE, current.stageCode)
            .UpdateAsync(new
            {
                clear_count     = current.clearCount + 1,
                last_clear_date = DateTime.UtcNow
            });

        return updated == 1 ? ErrorCode.None : ErrorCode.FailedUpdateData;
    }
}