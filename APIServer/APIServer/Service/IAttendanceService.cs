namespace APIServer.Service;

public interface IAttendanceService
{
    Task<Result> AttendanceAndReward(long userId);
}