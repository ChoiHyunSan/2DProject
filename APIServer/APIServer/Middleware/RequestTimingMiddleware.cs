using System.Diagnostics;
using APIServer.Log.Metrics;

namespace APIServer.Middleware;

public class RequestTimingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        // swagger/metrics는 제외
        var path = ctx.Request.Path.Value ?? "/";
        if (path.StartsWith("/metrics") || path.StartsWith("/swagger"))
        {
            await next(ctx);
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await next(ctx);
        }
        finally
        {
            sw.Stop();
            // 라우트 템플릿 (예: /api/users/{id}) 추출, 실패 시 실제 경로 사용
            var routePattern = (ctx.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText
                               ?? ctx.Request.Path.Value
                               ?? "unknown";

            MetricsRegistry.ApiRequestDuration
                .WithLabels(routePattern, ctx.Request.Method, ctx.Response.StatusCode.ToString())
                .Observe(sw.Elapsed.TotalSeconds);
        }
    }    
}