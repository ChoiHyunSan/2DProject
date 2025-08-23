using APIServer.Models.Entity;
using APIServer.Repository;
using static APIServer.JsonBodyParser;

namespace APIServer.Middleware;

public class RequestLockMiddleware(ILogger<RequestLockMiddleware> logger, RequestDelegate next, IMemoryDb memoryDb)
{
    private readonly ILogger<RequestLockMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;
    private readonly IMemoryDb _memoryDb = memoryDb;   
    
    /// <summary>
    /// 한 세션에 대한 요청을 동시에 최대 1번까지만 받도록 제한하는 미들웨어
    /// SessionCheckMiddleware 미들웨어를 진행한 이후에 진행 
    /// 
    /// 1) Session에 대한 Lock이 걸려있는지 확인
    /// 2-1) 걸려있지 않다면 Lock을 걸고서 그대로 진행
    /// 2-2) Lock이 이미 걸려있다면 요청을 실패로 처리
    /// 3) 요청을 처리한 이후에 Lock을 해제
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        var needToLock = context.Items["NeedToLock"] as bool? ?? false;
        var session = context.Items["userSession"] as UserSession;
        if (needToLock)
        {
            if (context.Items["userSession"] is not UserSession)
            {
                await SendErrorCode(context, ErrorCode.SessionNotFound, "Session Not Found");   
                return;
            }

            var lockResult = await _memoryDb.TrySessionRequestLock(session.userId);
            if (lockResult.IsFailed)
            {
                await SendErrorCode(context, lockResult.ErrorCode, "Session Request Lock Failed");
                return;
            }   
        }

        await _next(context);

        if (needToLock)
        {
            var unLockResult = await _memoryDb.TrySessionRequestUnLock(session.userId);
            if (unLockResult.IsFailed)
            {
                await SendErrorCode(context, unLockResult.ErrorCode, "Session Request UnLock Failed");
            }   
        }
    }
    
    
}