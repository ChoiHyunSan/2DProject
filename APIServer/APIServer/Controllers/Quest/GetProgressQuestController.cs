using APIServer.Models.DTO;
using APIServer.Models.DTO.Quest;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Quest;

[ApiController]
[Route("[controller]")]
public class GetProgressQuestController(ILogger<GetProgressQuestController> logger, IDataLoadService dataLoadService)
    : ControllerBase
{
    private readonly ILogger<GetProgressQuestController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;
    
    [HttpPost]
    public async Task<GetProgressQuestResponse> GetProgressQuestAsync([FromBody] GetProgressQuestRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetProgressQuest, "Request Get Progress Quest", new { session.email });

        var result = await _dataLoadService.GetProgressQuestList(session.userId, session.email, request.Pageable);
        return new GetProgressQuestResponse { code = result.ErrorCode, progressQuests = ConvertProgressQuest(result.Value) };
    }
    
    private static List<ProgressQuest> ConvertProgressQuest(List<UserQuestInprogress> quest)
    {
        return quest.Select(quest => new ProgressQuest
        {
            progress = quest.progress,
            questCode = quest.questCode,
        }).ToList();
    }
}