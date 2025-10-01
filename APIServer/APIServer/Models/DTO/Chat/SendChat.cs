using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO.Chat;

public class SendChatResponse : ResponseBase
{

}

public class SendChatRequest : RequestBase
{
    [Required]
    [MinLength(1, ErrorMessage = "CHAT MESSAGE CANNOT BE EMPTY")]
    private string message { get; set; } = string.Empty;    
}