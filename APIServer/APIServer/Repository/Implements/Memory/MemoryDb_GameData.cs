using APIServer.Models.DTO;
using CloudStructures.Structures;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<Result> CacheGameData(string email, GameData gameData)
    {
        var key = CreateGameDataKey(email);
        try
        {
            var handler = new RedisString<GameData>(_conn, key, null);
            _ = await handler.SetAsync(gameData, TimeSpan.FromMinutes(60));
            
            LogInfo(_logger, EventType.CacheGameData, "Cache Game Data", new { key });
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { email, ex.Message, ex.StackTrace });

            return ErrorCode.FailedCacheGameData;
        }
    }
}