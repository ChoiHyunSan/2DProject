using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnhanceController(ILogger<EnhanceController> logger, IInventoryService inventoryService)
    : ControllerBase
{
    private readonly ILogger<EnhanceController> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    
    [HttpPost]
    [Route("item")]
    public async Task<EnhanceItemResponse> EnhanceItemAsync([FromBody] EnhanceItemRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;

        LogInfo(_logger, EventType.EnhanceItem, "Request Enhance Item", new { session.userId, request.itemId });
        
        var errorCode = await _inventoryService.EnhanceItemAsync(session.userId, request.itemId);
        if (errorCode == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EnhanceItem, "Enhance Item Success", new { session.userId, request.itemId });
        }
        
        return new EnhanceItemResponse { code = errorCode };       
    }
    
    [HttpPost]
    [Route("rune")]
    public async Task<EnhanceRuneResponse> EnhanceRuneAsync([FromBody] EnhanceRuneRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EnhanceRune, "Request Enhance Rune", new { session.userId, request.runeId });
        
        var errorCode = await _inventoryService.EnhanceRuneAsync(session.userId, request.runeId);
        if (errorCode == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EnhanceRune, "Enhance Rune Success", new { session.userId, request.runeId });
        }
        
        return new EnhanceRuneResponse { code = errorCode };      
    }
}