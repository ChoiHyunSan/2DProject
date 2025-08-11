namespace APIServer.Models.Entity;

/// <summary>
/// 유저 세션 정보
/// 
/// </summary>
public class UserSession
{
    public long accountId { get; set; }                     // 계정 ID
    public long userId { get; set; }                        // 유저 ID
    public string authToken { get; set; } = string.Empty;   // 인증 토큰
    public string email { get; set; } = string.Empty;       // 이메일
    public DateTime createDate { get; set; }                // 세션 생성 시간
}