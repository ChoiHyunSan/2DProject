using System.Text;
using System.Text.Json;
using APIServer.Repository;

namespace APIServer.Middleware;

public class SessionCheckMiddleware(ILogger<SessionCheckMiddleware> _logger, IMemoryDb memoryDb, RequestDelegate next) 
{
    private readonly ILogger<SessionCheckMiddleware> _logger = _logger;   
    private readonly IMemoryDb _memoryDb = memoryDb;
    private readonly RequestDelegate _next = next;

    private readonly List<string> skipAuthorizeApiPaths =
    [
        "/api/login",
        "/api/register"    
    ];
    
    /// <summary>
    /// API 요청에 대한 세션 검증을 진행하는 미들웨어
    /// 1) API 경로에 대한 인증 필요 여부 확인 
    /// 2) Body에 있는 Email 정보를 토대로 세션 검색
    /// 3) 세션의 토큰을 대조하여 인증
    /// 4) 다음 Delegate 호출
    ///
    /// - 인증 제외 경로 목록은 skipAuthorizeApiPaths 리스트에 추가하여 사용한다.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        foreach (var path in skipAuthorizeApiPaths)
        {
            if (context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }        
        
        var email = await JsonBodyParser.GetStringValueAsync(context, "email");
        var authToken = await JsonBodyParser.GetStringValueAsync(context, "authToken");
        if (email is null || authToken is null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Parse Authorize Info Failed.");
            return;
        }
    
        var (errorCode , userSession) = await _memoryDb.GetSessionByEmail(email);
        if (errorCode != ErrorCode.None)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync($"Get Session Failed, ErrorCode : {errorCode}");
            return;
        }

        if (authToken != userSession.authToken)
        {
            _logger.LogInformation($"Request Token : {authToken}, Session Token : {userSession.authToken}");
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid Auth Token.");
            return;       
        }
        
        context.Items["userSession"] = userSession;       
        
        await _next(context);
    }
}