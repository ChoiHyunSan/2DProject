namespace APIServer.Service;

public interface IAttendanceService
{
    /// <summary> 출석체크 & 보상지급 </summary>
    Task<Result> AttendanceAndRewardAsync(long userId);
}