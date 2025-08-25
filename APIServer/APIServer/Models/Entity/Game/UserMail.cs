namespace APIServer.Models.Entity;

public class UserMail
{
    
    public long mail_id { get; set; }

    public string mail_title { get; set; } = string.Empty;

    public int reward_code { get; set; }

    public int count { get; set; } = -1;

    public bool earn_reward { get; set; } = false;

    public DateTime send_date { get; set; }

    public DateTime receive_date { get; set; }

    public DateTime expire_date { get; set; }
}
