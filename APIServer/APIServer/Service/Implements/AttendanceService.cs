using APIServer.Models.Entity;
using APIServer.Repository;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class AttendanceService(ILogger<AttendanceService> logger, IMasterDb masterDb, IGameDb gameDb, IMailService mailService) 
    : IAttendanceService
{
    private readonly ILogger<AttendanceService> _logger = logger;
    private readonly IMasterDb _masterDb = masterDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMailService _mailService = mailService;
    
    public async Task<Result> AttendanceAndReward(long userId)
    {
        if (await CheckAttendanceAlreadyComplete(userId) is var checkResult && checkResult.IsFailed)
        {
            return checkResult.ErrorCode;
        }
        var attendance = checkResult.Value;
        
        // 트랜잭잭션 처리
        var txResult = await _gameDb.WithTransactionAsync(async _ =>
        {
            // 출석 체크를 실행하고, 증가된 일수를 반환받습니다.
            var newAttendanceDay = await AttendanceToday(userId, attendance);
            if (newAttendanceDay == 0) 
            {
                return ErrorCode.FailedAttendance;
            }

            // 반환받은 출석 일수를 사용하여 보상을 지급합니다.
            if (await GetAttendanceRewardToday(userId, newAttendanceDay) == false)
            {
                return ErrorCode.FailedAttendanceReward;
            }

            return ErrorCode.None;
        });
        
        if(txResult != ErrorCode.None)
        {
            return txResult;
        }

        LogInfo(_logger, EventType.AttendanceCheck, "Attendance Check", new { userId });
        
        return ErrorCode.None;
    }

    private async Task<bool> GetAttendanceRewardToday(long userId, int day)
    {
        // 보상 조회
        var reward = _masterDb.GetAttendanceRewardMonths()[day];
        var code = reward.item_code;
        var price = reward.count;

        // 메일로 보상 송신
        if (await _mailService.SendRewardMail(userId, "출석 보상", code, price) is var sendReward && sendReward.IsFailed)
        {
            return false;
        }

        return true;
    }
    
    private async Task<int> AttendanceToday(long userId, UserAttendanceMonth attendance)
    {
        // 출석 일수를 1 증가시킵니다.
        int newAttendanceDay = attendance.last_attendance_date + 1;
    
        // 업데이트된 출석 정보를 DB에 반영합니다.
        await _gameDb.UpdateAttendanceToday(userId, newAttendanceDay);
    
        // 증가된 출석 일수를 반환합니다.
        return newAttendanceDay;
    }

    private async Task<Result<UserAttendanceMonth>> CheckAttendanceAlreadyComplete(long userId)
    {
        var attendance = await _gameDb.GetUserAttendance(userId);
        if (attendance is null)
        {
            return Result<UserAttendanceMonth>.Failure(ErrorCode.CannotFindUserAttendance);
        }

        // 출석체크가 완료되었는지 확인 (수정된 메서드 사용)
        if (CheckAttendanceAlreadyComplete(attendance.last_attendance_date))
        {
            return Result<UserAttendanceMonth>.Failure(ErrorCode.AttendanceAlreadyComplete);
        }
        
        // 금일 출석체크를 했는지 확인 (수정된 메서드 사용 및 인자 추가)
        if (CheckAttendanceAlreadyDoneToday(attendance.last_update_date))
        {
            return Result<UserAttendanceMonth>.Failure(ErrorCode.AttendanceAlreadyDoneToday);
        }
        
        return Result<UserAttendanceMonth>.Success(attendance);
    }

    private bool CheckAttendanceAlreadyComplete(int lastAttendanceDate)
    {
        // 현재 월의 총 일수
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
    
        // 사용자의 출석 일수와 현재 월의 총 일수가 같은지 확인
        return lastAttendanceDate == daysInMonth;
    }
    
    private bool CheckAttendanceAlreadyDoneToday(DateTime lastUpdateDate)
    {
        // 마지막 갱신 날짜가 오늘 날짜와 같은지 확인
        return lastUpdateDate.Date == DateTime.Now.Date;
    }

}