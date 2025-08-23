using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Quest;

[ApiController]
[Route("[controller]")]
public class RewardQuestController(ILogger<RewardQuestController> logger, IQuestService questService)
    : ControllerBase
{
    private readonly ILogger<RewardQuestController> _logger = logger;
    private readonly IQuestService _questService = questService;

    /// <summary>
    /// 
    /// 
    /// </summary>
    [HttpPost]
    public async Task<RewardQuestResponse> QuestRewardAsync([FromBody] RewardQuestRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.RewardQuest, "Request Quest Reward", new { session.userId });

        var result = await _questService.RewardQuest(session.userId, request.questCode);
        return new RewardQuestResponse { code = result.ErrorCode };       
    }
}
