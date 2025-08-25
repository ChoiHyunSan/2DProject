using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 월간 출석 보상 데이터
/// 테이블 : attendance_reward_month
/// </summary>
public class AttendanceRewardMonth
{
    public int day { get; set; }            // 출석 일자
    public long item_code { get; set; }      // 아이템 식별 코드
    public int count { get; set; }          // 아이템 보상 개수
}