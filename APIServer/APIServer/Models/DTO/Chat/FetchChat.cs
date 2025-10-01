namespace APIServer.Models.DTO.Chat;

public class FetchChatRequest : RequestBase
{
    public string latest { get; set; } = string.Empty;
    public int limit { get; set; }
}

public class FetchChatResponse : RequestBase
{
    public List<ChatMessage> messages { get; set; } = [];
}

public class ChatMessage
{
    public string messageId { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
}