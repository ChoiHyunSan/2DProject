using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO.Mail;

public class ReceiveMailRequest : RequestBase
{
    [Required]
    public long mailId { get; set; }
}

public class ReceiveMailResponse : ResponseBase
{
    
}