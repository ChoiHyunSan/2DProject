namespace APIServer;

using System.Text;
using System.Text.Json;

public static class JsonBodyParser
{
    public static async Task<string?> GetStringValueAsync(HttpContext context, string propertyName)
    {
        var ct = context.Request.ContentType;
        var isJson = ct != null && ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase);
        if (!isJson || context.Request.ContentLength is null or 0)
            return null;

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(
                   context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.String)
                return null;

            var value = prop.GetString()?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }
        catch
        {
            return null;
        }
    }
    
    public static async Task<string?> GetStringValueFromResponseAsync(Stream responseBodyStream, string propertyName)
    {
        using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, false, 1024, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            return null;

        return TryGetPropertyValue(body, propertyName);
    }
    
    private static string? TryGetPropertyValue(string json, string propertyName)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(propertyName, out var prop))
                return null;

            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.TryGetInt32(out var num) ? num.ToString() : null,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}
