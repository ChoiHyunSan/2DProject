using APIServer.Models.DTO.Inventory;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Inventory;

[ApiController]
[Route("api/[controller]")]
public class GetInventoryItemController(ILogger<GetInventoryItemController> logger, IDataLoadService dataLoadService)
    : ControllerBase
{
    private readonly ILogger<GetInventoryItemController> _logger = logger;
    private readonly IDataLoadService _dataLoadService = dataLoadService;

    /// <summary>
    /// 인벤토리 아이템 목록 조회 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 조회 요청 결과 (성공 : ErrorCode.None)
    /// - 페이징 된 아이템 목록
    /// </summary>
    [HttpPost]
    public async Task<GetInventoryItemResponse> GetInventoryItemAsync([FromBody] GetInventoryItemRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetInventoryItem, "Request Inventory Item", new { session.userId });

        var result = await _dataLoadService.GetInventoryItemListAsync(session.userId, request.Pageable);
        return new GetInventoryItemResponse { code = result.ErrorCode, items = result.Value };
    }
}