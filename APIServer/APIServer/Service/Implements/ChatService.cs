using APIServer.Models.DTO.Chat;
using APIServer.Models.Entity;
using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class ChatService(ILogger<ChatService> logger, IMemoryDb memoryDb, IGameDb gameDb)
    : IChatService
{
    private readonly ILogger<ChatService> _logger = logger;
    private readonly IMemoryDb _memoryDb = memoryDb;
    private readonly IGameDb _gameDb = gameDb;
    
    public async Task<Result<ChatMessage>> SendAsync(string email, string message)
    {
        try
        {
            // Redis
            var result = await _memoryDb.SendChatAsync(email, message);
            if (result == null)
            {
                return Result<ChatMessage>.Failure(ErrorCode.FailedSendChat);
            }

            // ms -> UTC
            var sendAtUtc = result.sendAt;
            var dash = result.messageId.IndexOf('-');
            if (dash > 0 && long.TryParse(result.messageId.AsSpan(0, dash), out var ms))
            {
                sendAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                result.sendAt = sendAtUtc;
            }

            // MySQL 
            var ok = await _gameDb.InsertChatLogAsync(new ChatLog
            {
                redis_stream_id = result.messageId,
                email           = result.email,
                message         = result.message,
                send_at_utc     = sendAtUtc
            });

            if (!ok)
            {
                LogError(_logger, ErrorCode.FailedSendChat, EventType.SendChat,
                    "InsertChatLogAsync returned false",
                    new { result.messageId, email, message });

                return Result<ChatMessage>.Failure(ErrorCode.FailedSendChat);
            }
            
            return Result<ChatMessage>.Success(result);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedSendChat, EventType.SendChat,
                "Failed to send chat message",
                new { email, message, ex.Message, ex.StackTrace });

            return Result<ChatMessage>.Failure(ErrorCode.FailedSendChat);
        }
    }
    
    private static bool IsDuplicateKey(Exception ex)
    {
        var m = ex.Message?.ToLowerInvariant() ?? "";
        return m.Contains("duplicate") || m.Contains("unique")
                                       || m.Contains("pk") || m.Contains("primary key");
    }

    public async Task<Result<List<ChatMessage>>> FetchAsync(int limit, string? afterMessageId)
    {
        try
        {
            var result = await _memoryDb.FetchChatsAsync(limit, afterMessageId);
            if (result == null)
            {
                return Result<List<ChatMessage>>.Failure(ErrorCode.FailedFetchChat); 
            }
            
            return Result<List<ChatMessage>>.Success(result); 
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedFetchChat, EventType.SendChat,
                "Failed to fetch chat message", new { limit, afterMessageId , ex.Message, ex.StackTrace});
            return Result<List<ChatMessage>>.Failure(ErrorCode.FailedFetchChat);  
        }
    }
}