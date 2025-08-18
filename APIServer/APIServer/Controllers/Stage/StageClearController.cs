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

    [HttpPost]
    public async Task<StageClearResponse> Post([FromBody] StageClearRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.ClearStage, "Request Clear Stage", new { session.email, request.stageCode });
        
        var result = await _stageService.ClearStage(session.email, request.stageCode);
        return new StageClearResponse { code = result.ErrorCode };
    }
}