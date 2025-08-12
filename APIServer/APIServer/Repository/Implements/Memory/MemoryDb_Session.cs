using APIServer.Models.Entity;
using CloudStructures.Structures;
using static APIServer.ErrorCode;
using static APIServer.EventType;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<ErrorCode> RegisterSessionAsync(UserSession session)
    {
        var key = CreateSessionKey(session.email);
        try
        {
            var handle = new RedisString<UserSession>(_conn, key, null);
            _ = await handle.SetAsync(session, TimeSpan.FromMinutes(60));
            
            LogInfo(_logger, RegisterSession, "Register Session", new { key });
        }
        catch (Exception e)
        {
            LogError(_logger, FailedRegisterSession, RegisterSession, "Register Session Failed",new
            {
                session.email,
                e.Message,
                e.StackTrace
            });
            return FailedRegisterSession;
        }
        
        return None;
    }

    public async Task<(ErrorCode, UserSession)> GetSessionByEmail(string email)
    {
        var key = CreateSessionKey(email);
        try
        {
            var handle = new RedisString<UserSession>(_conn, key, null);
            var data = await handle.GetAsync();
            if (data.HasValue)
            {
                return (None, data.Value);
            }

            return (SessionNotFound, new UserSession());
        }
        catch (Exception e)
        {
            LogError(_logger, FailedGetSession, GetSession, "Get Session By Email Failed",new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return (FailedGetSession, new UserSession());
        }
    }
}