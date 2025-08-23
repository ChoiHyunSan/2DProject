using APIServer.Models.DTO;
using APIServer.Models.Entity;
using CloudStructures.Structures;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<bool> CacheGameData(long userId, GameData gameData)
    {
        var key = CreateGameDataKey(userId);
        try
        {
            var handler = new RedisString<GameData>(_conn, key, null);
            
            return await handler.SetAsync(gameData, TimeSpan.FromMinutes(60));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { email = userId, ex.Message, ex.StackTrace });

            return false;
        }
    }

    public async Task<Result<List<UserQuestInprogress>>> GetCachedQuestList(long userId)
    {
        var key = CreateQuestKey(userId);
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            var result =  await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<List<UserQuestInprogress>>.Success(result.Value);
            }
            
            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.CannotFindQuestList);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { userId, ex.Message, ex.StackTrace });

            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.FailedCacheGameData);
        }     
    }

    public async Task<Result> CacheQuestList(long userId, List<UserQuestInprogress> progressList)
    {
        var key = CreateQuestKey(userId);
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            var result = await handler.SetAsync(progressList, TimeSpan.FromMinutes(60));
            if (result == false)
            {
                return Result.Failure(ErrorCode.FailedCacheGameData);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }
}