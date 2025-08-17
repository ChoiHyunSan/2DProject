using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController(ILogger<EquipmentController> logger, IInventoryService inventoryService)
    : ControllerBase
{
    private readonly ILogger<EquipmentController> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;

    /// <summary>
    /// 아이템 장착 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 장착 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("item")]   
    public async Task<EquipmentItemResponse> EquipmentItemAsync([FromBody] EquipmentItemRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EquipItem, "Request Equipment Item", new { session.userId, request.characterId, request.itemId });

        var errorCode = await _inventoryService.EquipItemAsync(session.userId, request.characterId, request.itemId);
        if (errorCode == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipItem, "Equipment Item Success", new { session.userId, request.characterId, request.itemId });
        }
        
        return new EquipmentItemResponse { code = errorCode };       
    }

    /// <summary>
    /// 룬 장착 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 장착 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("rune")]  
    public async Task<EquipmentRuneResponse> EquipmentRuneAsync([FromBody] EquipmentRuneRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EquipRune, "Request Equipment Rune", new { session.userId, request.characterId, request.runeId });

        var errorCode = await _inventoryService.EquipRuneAsnyc(session.userId, request.characterId, request.runeId);
        if (errorCode == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipRune, "Equipment Item Success", new { session.userId, request.characterId,  request.runeId });
        }
        
        return new EquipmentRuneResponse { code = errorCode };   
    }
}