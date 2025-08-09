using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 월간 출석 정보 테이블 데이터 
/// 테이블 : user_attendance_month
/// </summary>
public class UserAttendanceMonth
{
    [Column("last_attendance_date")]
    public int lastAttendanceDate { get; set; }     // 최신 출석 일수
    
    [Column("start_update_date")]
    public DateTime startUpdateDate { get; set; }   // 첫 갱신 시간
    
    [Column("last_update_date")]
    public DateTime lastUpdateDate { get; set; }    // 마지막 갱신 시간
}

/// <summary>
/// 유저 주간 출석 정보 테이블 데이터 
/// 테이블 : user_attendance_week
/// </summary>
public class UserAttendanceWeek
{                                                   
    [Column("last_attendance_date")]
    public int lastAttendanceDate { get; set; }     // 최신 출석 일수
    
    [Column("start_update_date")]
    public DateTime startUpdateDate { get; set; }   // 첫 갱신 시간
    
    [Column("last_update_date")]
    public DateTime lastUpdateDate { get; set; }    // 마지막 갱신 시간
}

