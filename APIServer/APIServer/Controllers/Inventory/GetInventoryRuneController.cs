using APIServer.Models.DTO;
using APIServer.Models.DTO.Inventory;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Inventory;

[ApiController]
[Route("[controller]")]
public class GetInventoryRuneController(ILogger<GetInventoryRuneController> logger, IDataLoadService dataLoadService)
    : ControllerBase
{
    private readonly ILogger<GetInventoryRuneController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;

    /// <summary>
    /// 인벤토리 룬 목록 조회 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 목록 조회 요청 결과 (성공 : ErrorCode.None)
    /// - 페이징된 룬 목록
    /// </summary>
    [HttpPost]
    public async Task<GetInventoryRuneResponse> GetInventoryRuneAsync([FromBody] GetInventoryRuneRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetInventoryRune, "Request Inventory Rune", new { session.userId });
        
        var result = await _dataLoadService.GetInventoryRuneListAsync(session.userId, request.Pageable);
        return new GetInventoryRuneResponse { code = result.ErrorCode, runes = result.Value };       
    }
}