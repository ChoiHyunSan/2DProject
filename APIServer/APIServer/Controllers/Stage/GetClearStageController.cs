using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GetClearStageController(ILogger<GetClearStageController> logger, IStageService stageService)
    : ControllerBase
{
    private readonly ILogger<GetClearStageController> _logger = logger;
    private readonly IStageService _stageService = stageService;

    /// <summary>
    /// 클리어 스테이지 목록 조회 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 조회 요청 결과 (성공 : ErrorCode.None)
    /// - 스테이지 정보 리스트 : StageList 
    /// </summary>
    [HttpPost]
    public async Task<GetClearStageResponse> GetClearStageAsync([FromBody] GetClearStageRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetClearStage, "Request Get Clear Stage", new { session.userId });

        var result = await _stageService.GetClearStage(session.userId);
        return new GetClearStageResponse { code = result.ErrorCode, stageList = result.Value };
    }
}