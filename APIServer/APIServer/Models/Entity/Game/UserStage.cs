using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 스테이지 클리어 정보 관리
/// 테이블 : user_clear_stage
/// </summary>
public class UserClearStage
{
    public long user_id { get; set; }                    // 유저 ID
    public int stage_code { get; set; }                  // 스테이지 식별 코드
    public int clear_count { get; set; }                 // 스테이지 클리어 횟수
    public DateTime first_clear_date { get; set; }       // 첫 클리어 날짜
    public DateTime last_clear_date { get; set; }        // 가장 최근 클리어 날짜
}
