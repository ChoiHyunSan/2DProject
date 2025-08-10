using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 월간 출석 보상 데이터
/// 테이블 : attendance_reward_month
/// </summary>
public class AttendanceRewardMonth
{
    [Column("day")]
    public int day { get; set; }            // 출석 일자
    
    [Column("item_code")]
    public long itemCode { get; set; }      // 아이템 식별 코드
    
    [Column("count")]
    public int count { get; set; }          // 아이템 보상 개수
}

/// <summary>
/// 주간 출석 보상 데이터
/// 테이블 : attendance_reward_week
/// </summary>
public class AttendanceRewardWeek
{
    [Column("day")]
    public int day { get; set; }            // 출석 일자
    
    [Column("item_code")]
    public long itemCode { get; set; }      // 아이템 식별 코드
    
    [Column("count")]
    public int count { get; set; }          // 아이템 보상 개수
}