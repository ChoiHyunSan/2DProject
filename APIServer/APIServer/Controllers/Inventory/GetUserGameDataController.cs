using APIServer.Models.DTO.Inventory;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers.Inventory;

[ApiController]
[Route("[controller]")]
public class GetUserGameDataController(ILogger<GetUserGameDataController> logger, IDataLoadService dataLoadService)
: ControllerBase
{
    private readonly ILogger<GetUserGameDataController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;

    /// <summary>
    /// 유저 게임 데이터 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 요청 결과 (성공 : ErrorCode.None)
    /// - GameData : 유저 데이터
    /// </summary>
    [HttpPost]
    public async Task<GetUserGameDataResponse> GetUserGameDataAsync([FromBody] GetUserGameDataRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LoggerManager.LogInfo(_logger, EventType.GetUserGameData, "Request Get User Game Data", new { session.userId });
        
        var result = await _dataLoadService.GetUserGameDataAsync(session.userId);
        if (result.IsFailed)
        {
            return new GetUserGameDataResponse {code = result.ErrorCode};       
        }
        return new GetUserGameDataResponse { code = result.ErrorCode, gameData = GameData.Of(result.Value)};       
    }
}