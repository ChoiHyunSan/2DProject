namespace APIServer.Models.DTO.Mail;

public class ReceiveMailRequest : RequestBase
{
    public long mailId { get; set; }
}

public class ReceiveMailResponse : ResponseBase
{
    
}