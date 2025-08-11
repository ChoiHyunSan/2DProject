using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 스테이지 클리어 정보 관리
/// 테이블 : user_clear_stage
/// </summary>
public class UserClearStage
{
    [Column("stage_code")]
    public int stageCode { get; set; }                  // 스테이지 식별 코드
    
    [Column("clear_count")]
    public int clearCount { get; set; }                 // 스테이지 클리어 횟수
    
    [Column("first_clear_date")]
    public DateTime firstClearDate { get; set; }        // 첫 클리어 날짜
    
    [Column("last_clear_date")]
    public DateTime lastClearDate { get; set; }         // 가장 최근 클리어 날짜
}