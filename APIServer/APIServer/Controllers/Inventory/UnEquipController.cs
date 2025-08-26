using APIServer.Models.DTO;
using APIServer.Models.DTO.Inventory;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Inventory;

public class UnEquipController(ILogger<UnEquipController> logger, IInventoryService inventoryService)
: ControllerBase
{
    private readonly ILogger<UnEquipController> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;

    /// <summary>
    /// 아이템 장착 해제 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 장착해제 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("item")]   
    public async Task<UnEquipItemResponse> UnEquipmentItemAsync([FromBody] UnEquipItemRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EquipItem, "Request UnEquipment Item", new { session.userId, request.characterId, request.itemId });

        var result = await _inventoryService.UnEquipItemAsync(session.userId, request.characterId, request.itemId);
        return new UnEquipItemResponse { code = result.ErrorCode };       
    }

    /// <summary>
    /// 룬 장착 해제 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 장착해제 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("rune")]  
    public async Task<UnEquipRuneResponse> UnEquipmentRuneAsync([FromBody] UnEquipRuneRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EquipRune, "Request UnEquipment Rune", new { session.userId, request.characterId, request.runeId });

        var result = await _inventoryService.UnEquipRuneAsnyc(session.userId, request.characterId, request.runeId);
        return new UnEquipRuneResponse { code = result.ErrorCode };   
    }
}