using System.Diagnostics;
using APIServer.Metrics;

namespace APIServer.Middleware;

public class RequestMetricsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var method = ctx.Request.Method ?? "UNKNOWN";
        var path = ResolveRouteTemplate(ctx) ?? ctx.Request.Path.Value ?? "UNKNOWN";

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            // 예외 카운트(엔드포인트/메서드 라벨로 누적)
            MetricRegistry.RecordException(ex, path, method);
            throw;
        }
        finally
        {
            sw.Stop();
            var code = (ctx.Response?.StatusCode ?? 0).ToString();
            MetricRegistry.ApiRequestDuration
                .WithLabels(path, method, code)
                .Observe(sw.Elapsed.TotalSeconds);
        }
    }

    private static string? ResolveRouteTemplate(HttpContext ctx)
    {
        // 라우트 템플릿
        if (ctx.GetEndpoint() is RouteEndpoint re)
            return re.RoutePattern.RawText;  
        return null;
    }
}