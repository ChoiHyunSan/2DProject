using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Attendance;

[ApiController]
[Route("api/[controller]")]
public class AttendanceCheckController(ILogger<AttendanceCheckController> logger, IAttendanceService attendanceService)
    : ControllerBase
{
    private readonly ILogger<AttendanceCheckController> _logger = logger;
    private readonly IAttendanceService _attendanceService = attendanceService;
    
    /// <summary>
    /// 출석 체크 요청 API
    /// 세션 인증 : O
    /// 반환 값 : 출석 요청 결과
    /// </summary>
    [HttpPost]
    public async Task<AttendanceResponse> AttendanceCheckAsync([FromBody] AttendanceRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.AttendanceCheck, "Attendance Check", new { request });

        var result = await _attendanceService.AttendanceAndRewardAsync(session.userId);
        return new AttendanceResponse { code = result.ErrorCode };       
    }
}