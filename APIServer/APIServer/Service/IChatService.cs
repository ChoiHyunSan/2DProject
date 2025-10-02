using APIServer.Models.DTO.Chat;

namespace APIServer.Service;

public interface IChatService
{
    /// <summary> 채팅 송신 </summary>
    Task<Result<ChatMessage>> SendAsync(string email, string message);
    
    /// <summary> 채팅 불러오기 </summary>
    Task<Result<List<ChatMessage>>> FetchAsync(int limit, string? afterMessageId);   
}