using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO.Chat;

public class SendChatResponse : ResponseBase
{
    public string messageId { get; set; } = string.Empty;   
}

public class SendChatRequest : RequestBase
{
    [Required]
    [MinLength(1, ErrorMessage = "CHAT MESSAGE CANNOT BE EMPTY")]
    public string message { get; set; } = string.Empty;    
}