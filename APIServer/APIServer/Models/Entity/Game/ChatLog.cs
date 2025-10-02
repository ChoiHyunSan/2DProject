namespace APIServer.Models.Entity;

/// <summary>
/// 채팅 로그 테이블
/// 테이블 : chat_log
/// </summary>
public class ChatLog
{
    public string   redis_stream_id { get; set; } = string.Empty;
    public string   email           { get; set; } = string.Empty;
    public string   message         { get; set; } = string.Empty;
    public DateTime send_at_utc     { get; set; }          
}