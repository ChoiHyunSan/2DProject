using APIServer.Models.Redis;
using CloudStructures.Structures;
using ZLogger;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<Result> CacheStageInfo(InStageInfo inStageInfo)
    {
        var key = CreateStageInfoKey(inStageInfo.email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            _ = await handler.SetAsync(inStageInfo, TimeSpan.FromMinutes(60));

            LoggerManager.LogInfo(_logger, EventType.CacheStageInfo, "Cache Stage Info", new { key });
            return Result.Success();
        }
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedCacheStageInfo, EventType.CacheStageInfo,
                "Cache Stage Info", new { userId = inStageInfo.email, e.Message, e.StackTrace });

            return ErrorCode.FailedCacheStageInfo;
        }
    }

    public async Task<Result<InStageInfo>> GetGameInfo(string email)
    {
        var key = CreateStageInfoKey(email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            var inGameInfo = await handler.GetAsync();
            if (!inGameInfo.HasValue)
            {
                LoggerManager.LogError(_logger, ErrorCode.CannotLoadStageInfo, EventType.LoadStageInfo,
                    "Load Stage Info", new { email });
                return Result<InStageInfo>.Failure(ErrorCode.CannotLoadStageInfo);
            }

            return Result<InStageInfo>.Success(inGameInfo.Value);
        }
        
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedLoadStageInfo, EventType.LoadStageInfo,
                "Load Stage Info", new { email });
            return Result<InStageInfo>.Failure(ErrorCode.FailedLoadStageInfo);
        }
    }

    public async Task<Result> DeleteStageInfo(InStageInfo stageInfo)
    {
        var key = CreateStageInfoKey(stageInfo.email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            var deleteResult = await handler.DeleteAsync();
            if (!deleteResult)
            {
                LoggerManager.LogError(_logger, ErrorCode.FailedDeleteStageInfo, EventType.DeleteStageInfo,
                    "Failed Delete Stage Info", new { stageInfo.email });
                return Result.Failure(ErrorCode.FailedDeleteStageInfo);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedDeleteStageInfo, EventType.DeleteStageInfo,
                "Failed Delete Stage Info", new { stageInfo.email });
            return Result.Failure(ErrorCode.FailedDeleteStageInfo);
        }
    }
}