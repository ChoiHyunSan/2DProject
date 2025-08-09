using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 계정 정보 테이블 데이터 
/// 테이블 : user_account
/// </summary>
public class UserAccount
{
    [Column("account_id")]
    public long accountId { get; set; }                     // 인증 계정 ID
    
    [Column("user_id")]
    public long userId { get; set; }                        // 유저 ID
    
    [Column("email")]
    public string email { get; set; } = string.Empty;       // 이메일
    
    [Column("password")]
    public string password { get; set; } = string.Empty;    // 비밀번호 
    
    [Column("salt_value")]
    public string saltValue { get; set; } = string.Empty;   // 비밀번호 솔트 값
    
    [Column("create_date")]
    public DateTime createDate { get; set; }                // 계정 생성 날짜
}
