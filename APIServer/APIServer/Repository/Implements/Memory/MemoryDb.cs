using APIServer.Config;
using APIServer.Models.Entity;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

public class MemoryDb(IOptions<DbConfig> config, ILogger<MemoryDb> logger)
    : RedisBase(config.Value.Redis), IMemoryDb
{
    private readonly ILogger<MemoryDb> _logger = logger;
    
    public async Task<bool> RegisterSessionAsync(UserSession session)
    {
        string key = $"SESSION_{session.email}";
        try
        {
            var handle = new RedisString<UserSession>(_conn, key, null);
            await handle.SetAsync(session, TimeSpan.FromMinutes(60));
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedRegisterSession, EventType.RegisterSession, "Register Session Failed",new
            {
                session.email,
                e.Message,
                e.StackTrace
            });
            return false;
        }
        
        return true;
    }
}