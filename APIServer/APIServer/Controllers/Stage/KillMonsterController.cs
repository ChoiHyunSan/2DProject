using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KillMonsterController(ILogger<KillMonsterController> logger, IStageService stageService)
    : ControllerBase
{
    private readonly ILogger<KillMonsterController> _logger = logger;
    private readonly IStageService _stageService = stageService;

    [HttpPost]
    public async Task<KillMonsterResponse> KillMonsterAsync([FromBody] KillMonsterRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;

        LoggerManager.LogInfo(_logger, EventType.KillMonster, "Request Kill Monster",
            new { request.email, request.monsterCode });

        var result = await _stageService.KillMonster(session.email, request.monsterCode);
        return new KillMonsterResponse { code = result.ErrorCode };
    }
}