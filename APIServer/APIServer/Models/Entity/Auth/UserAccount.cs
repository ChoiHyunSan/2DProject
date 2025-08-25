using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 계정 정보 테이블 데이터 
/// 테이블 : user_account
/// </summary>
public class UserAccount
{
    public long account_id { get; set; }                     // 인증 계정 ID
    public long user_id { get; set; }                        // 유저 ID
    public string email { get; set; } = string.Empty;       // 이메일
    public string password { get; set; } = string.Empty;    // 비밀번호 
    public string salt_value { get; set; } = string.Empty;  // 비밀번호 솔트 값
    public DateTime create_date { get; set; }                // 계정 생성 날짜
}
