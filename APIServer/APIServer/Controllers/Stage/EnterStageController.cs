using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnterStageController(ILogger<EnterStageController> logger, IStageService stageService)
    : ControllerBase
{
    private readonly ILogger<EnterStageController> _logger = logger;
    private readonly IStageService _stageService = stageService;
    
    /// <summary>
    /// 스테이지 입장 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 입장 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    public async Task<EnterStageResponse> EnterStageAsync([FromBody] EnterStageRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EnterStage, "Request Enter Stage", new { session.email, request.stageCode });
        
        var result = await _stageService.EnterStage(session.userId, session.email, request.stageCode, request.characterIds);
        return new EnterStageResponse { code = result.ErrorCode, monsterList = result.Value };
    }
}