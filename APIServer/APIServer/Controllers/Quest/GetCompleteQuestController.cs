using APIServer.Models.DTO.Quest;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Quest;

[ApiController]
[Route("[controller]")]
public class GetCompleteQuestController(ILogger<GetProgressQuestController> logger, IDataLoadService dataLoadService)
    : ControllerBase
{
    private readonly ILogger<GetProgressQuestController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;
    
    /// <summary>
    /// 완료된 퀘스트 목록 조회 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 요청 결과 (성공 : ErrorCode.None)
    /// - 퀘스트 목록 
    /// </summary>
    [HttpPost]
    public async Task<GetCompleteQuestResponse> GetCompleteQuestAsync([FromBody] GetCompleteQuestRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetCompleteQuest, "Request Get Complete Quest", new { session.userId });

        var result = await _dataLoadService.GetCompleteQuestListAsync(session.userId, request.Pageable);
        return new GetCompleteQuestResponse { code = result.ErrorCode, completeQuests = ConvertCompleteQuest(result.Value) };
    }
    
    private static List<CompleteQuest> ConvertCompleteQuest(List<UserQuestComplete> quest)
    {
        return quest.Select(quest => new CompleteQuest
        {
            questCode = quest.questCode,
        }).ToList();
    }
}