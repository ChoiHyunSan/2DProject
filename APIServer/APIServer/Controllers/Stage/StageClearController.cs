using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StageClearController(ILogger<StageClearController> logger, IStageService stageService)
    : ControllerBase
{
    private readonly ILogger<StageClearController> _logger = logger;
    private readonly IStageService _stageService = stageService;

    /// <summary>
    /// 스테이지 클리어 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 스테이지 클리어 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    public async Task<StageClearResponse> StageClearAsync([FromBody] StageClearRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.ClearStage, "Request Clear Stage", new { session.email, request.stageCode });
        
        var result = await _stageService.ClearStage(session.userId, request.stageCode);
        return new StageClearResponse { code = result.ErrorCode };
    }
}