using APIServer.Models.DTO.Chat;
using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class ChatService(ILogger<ChatService> logger, IMemoryDb memoryDb)
    : IChatService
{
    private readonly ILogger<ChatService> _logger = logger;
    private readonly IMemoryDb _memoryDb = memoryDb;

    public async Task<Result<ChatMessage>> SendAsync(string email, string message)
    {
        try
        {
            var result = await _memoryDb.SendChatAsync(email, message);
            if (result == null)
            {
                return Result<ChatMessage>.Failure(ErrorCode.FailedSendChat);  
            }
            
            return Result<ChatMessage>.Success(result);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedSendChat, EventType.SendChat,
                "Failed to send chat message", new { email, message , ex.Message, ex.StackTrace});
            return Result<ChatMessage>.Failure(ErrorCode.FailedSendChat);   
        }
    }

    public async Task<Result<List<ChatMessage>>> FetchAsync(int limit, string? afterMessageId)
    {
        try
        {
            var result = await _memoryDb.FetchChatsAsync(limit, afterMessageId);
            
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