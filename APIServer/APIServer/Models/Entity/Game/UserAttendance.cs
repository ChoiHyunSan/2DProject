using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 월간 출석 정보 테이블 데이터 
/// 테이블 : user_attendance_month
/// </summary>
public class UserAttendanceMonth
{
    public int last_attendance_date { get; set; }     // 최신 출석 일수
    public DateTime start_update_date { get; set; }   // 첫 갱신 시간
    public DateTime last_update_date { get; set; }    // 마지막 갱신 시간
}
