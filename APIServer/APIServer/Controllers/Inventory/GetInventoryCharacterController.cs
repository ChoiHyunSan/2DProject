using APIServer.Models.DTO.Inventory;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
public class GetInventoryCharacterController(ILogger<GetInventoryCharacterController> logger, IDataLoadService dataLoadService)
    : ControllerBase
{
    private readonly ILogger<GetInventoryCharacterController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;

    /// <summary>
    /// 캐릭터 목록 조회 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 목록 조회 요청 결과 (성공 : ErrorCode.None)
    /// - 캐릭터 전체 목록
    /// </summary>
    [HttpPost]
    public async Task<GetInventoryCharacterResponse> GetInventoryCharacterAsync([FromBody] GetInventoryCharacterRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetInventoryCharacter, "Request Inventory Character", new { session.userId });

        var result = await _dataLoadService.GetInventoryCharacterListAsync(session.userId);
        return new GetInventoryCharacterResponse { code = result.ErrorCode, characters = result.Value };
    }
}