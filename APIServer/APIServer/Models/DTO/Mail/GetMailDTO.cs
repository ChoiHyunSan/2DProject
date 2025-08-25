namespace APIServer.Models.DTO.Mail;

public class GetMailRequest : PageableBase
{
    
}

public class GetMailResponse : ResponseBase
{
    public List<MailData> mails { get; set; } = [];
}

public class MailData
{
    public long mailId { get; set; }
    public string title { get; set; }
    public long rewardCode { get; set; }
    public int rewardCount { get; set; }
    public bool isReceive { get; set; }
    public DateTime sendDate { get; set; }
    public DateTime expireDate { get; set; }
}