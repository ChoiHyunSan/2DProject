using System.Text;
using System.Text.Json;

namespace APIServer.Middleware;

public class ResponseStatusCodeMiddleware(ILogger<ResponseStatusCodeMiddleware> logger, RequestDelegate next)
{
    private readonly ILogger<ResponseStatusCodeMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Response에서 공통적으로 들어가는 ErrorCode를 통해 Response의 Status Code를 변경한다.
    /// 0) Request 그대로 진행
    /// 1) Body에서 ErrorCode를 Parse
    /// 2) ErrorCode에 맞는 Status Code 탐색
    /// 3) Response의 Status Code 값을 수정
    /// 
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context);

        try
        {
            buffer.Position = 0;

            if (context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                // code 값 읽기
                var codeStr = await JsonBodyParser.GetStringValueFromResponseAsync(buffer, "code");
                if (codeStr != null && int.TryParse(codeStr, out var codeValue))
                {
                    // HTTP 상태코드 변경
                    context.Response.StatusCode = codeValue % 1000;

                    // JSON의 code 값 변경 (마지막 3자리 제거)
                    buffer.Position = 0;
                    using var doc = await JsonDocument.ParseAsync(buffer);

                    var root = doc.RootElement.Clone();
                    using var ms = new MemoryStream();
                    await using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
                    {
                        writer.WriteStartObject();
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.NameEquals("code"))
                                writer.WriteNumber("code", codeValue / 1000);
                            else
                                prop.WriteTo(writer);
                        }
                        writer.WriteEndObject();
                    }

                    // 바꾼 JSON 전송
                    context.Response.ContentLength = ms.Length;
                    context.Response.Body = originalBody;
                    ms.Position = 0;
                    await ms.CopyToAsync(context.Response.Body);
                    return;
                }
            }

            // JSON이 아니거나 code가 없으면 그대로 전달
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change response status code");
            return;
        }
    }
}