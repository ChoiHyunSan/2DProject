using APIServer.Models.Redis;
using CloudStructures.Structures;
using ZLogger;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<bool> CacheStageInfo(InStageInfo inStageInfo)
    {
        var key = CreateStageInfoKey(inStageInfo.email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            
            return await handler.SetAsync(inStageInfo, TimeSpan.FromMinutes(60));
        }
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedCacheStageInfo, EventType.CacheStageInfo,
                "Failed Cache Stage Info", new { userId = inStageInfo.email, e.Message, e.StackTrace });

            return false;
        }
    }

    public async Task<Result<InStageInfo>> GetGameInfo(string email)
    {
        var key = CreateStageInfoKey(email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            var data = await handler.GetAsync();
            if (data.HasValue)
            {
                return Result<InStageInfo>.Success(data.Value);
            }

            return Result<InStageInfo>.Failure(ErrorCode.CannotFindInStageInfo);
        }
        
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedLoadStageInfo, EventType.LoadStageInfo,
                "Failed Get Stage Info", new { email });
            
            return Result<InStageInfo>.Failure(ErrorCode.FailedLoadStageInfo);
        }
    }

    public async Task<bool> DeleteStageInfo(InStageInfo stageInfo)
    {
        var key = CreateStageInfoKey(stageInfo.email);
        try
        {
            var handler = new RedisString<InStageInfo>(_conn, key, null);
            
            return await handler.DeleteAsync();
        }
        catch (Exception e)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedDeleteStageInfo, EventType.DeleteStageInfo,
                "Failed Delete Stage Info", new { stageInfo.email });

            return false;
        }
    }
}