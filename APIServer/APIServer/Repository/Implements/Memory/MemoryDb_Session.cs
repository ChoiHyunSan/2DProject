using APIServer.Models.Entity;
using CloudStructures.Structures;
using StackExchange.Redis;
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
            
            LogInfo(_logger, EventType.RegisterSession, "Register Session", new { key });
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedRegisterSession, EventType.RegisterSession, "Register Session Failed",new
            {
                session.email,
                e.Message,
                e.StackTrace
            });
            return ErrorCode.FailedRegisterSession;
        }
        
        return ErrorCode.None;
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
                return (ErrorCode.None, data.Value);
            }

            return (ErrorCode.SessionNotFound, new UserSession());
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedGetSession, EventType.GetSession, "Get Session By Email Failed",new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return (ErrorCode.FailedGetSession, new UserSession());
        }
    }

    public async Task<ErrorCode> TrySessionRequestLock(string email, TimeSpan? ttl = null)
    {
        var key    = CreateSessionLockKey(email);
        var expiry = ttl ?? TimeSpan.FromSeconds(5); 
        
        try
        {
            var handle = new RedisString<string>(_conn, key, null);

            // 락의 소유 토큰(필요시 unlock 검증에 활용 가능)
            var token = $"{Environment.MachineName}:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            // NX + TTL: 이미 있으면 false
            var ok = await handle.SetAsync(token, expiry, when: When.NotExists);
            if (!ok)
            {
                LogInfo(_logger, EventType.SessionLock, "Session lock already exists", new { key, ttl = expiry.TotalSeconds });
                return ErrorCode.AlreadySessionLock; 
            }

            LogInfo(_logger, EventType.SessionLock, "Acquire session lock", new { key, ttl = expiry.TotalSeconds });
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedSessionLock, EventType.SessionLock, "Acquire session lock failed", new
            {
                email, key, e.Message, e.StackTrace
            });
            return ErrorCode.FailedSessionLock;
        }
    }

    public async Task<ErrorCode> TrySessionRequestUnLock(string email)
    {
        var key = CreateSessionLockKey(email);

        try
        {
            var handle  = new RedisString<string>(_conn, key, null);
            var deleted = await handle.DeleteAsync(); // 존재하면 true, 없으면 false

            if (!deleted)
            {
                LogInfo(_logger, EventType.SessionUnLock, "Session lock not found on unlock", new { key });
                return ErrorCode.SessionLockNotFound;
            }

            LogInfo(_logger, EventType.SessionUnLock, "Release session lock", new { key });
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedSessionUnLock, EventType.SessionUnLock, "Release session lock failed", new
            {
                email, key, e.Message, e.StackTrace
            });
            return ErrorCode.FailedSessionUnLock;
        }
    }
}