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
    
    /// <summary>
    /// 진행중인 퀘스트 목록 조회 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 요청 결과 (성공 : ErrorCode.None)
    /// - 퀘스트 목록 
    /// </summary>
    [HttpPost]
    public async Task<GetProgressQuestResponse> GetProgressQuestAsync([FromBody] GetProgressQuestRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetProgressQuest, "Request Get Progress Quest", new { session.email });

        var result = await _dataLoadService.GetProgressQuestListAsync(session.userId, request.Pageable);
        return new GetProgressQuestResponse { code = result.ErrorCode, progressQuests = ConvertProgressQuest(result.Value) };
    }
    
    private static List<ProgressQuest> ConvertProgressQuest(List<UserQuestInprogress> quest)
    {
        return quest.Select(quest => new ProgressQuest
        {
            progress = quest.progress,
            questCode = quest.quest_code,
        }).ToList();
    }
}