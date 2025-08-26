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
    
    /// <summary>
    /// 아이템 강화 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 강화 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("item")]
    public async Task<EnhanceItemResponse> EnhanceItemAsync([FromBody] EnhanceItemRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;

        LogInfo(_logger, EventType.EnhanceItem, "Request Enhance Item", new { session.userId, request.itemId });
        
        var result = await _inventoryService.EnhanceItemAsync(session.userId, request.itemId);
        return new EnhanceItemResponse { code = result.ErrorCode };       
    }
    
    /// <summary>
    /// 룬 강화 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 강화 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("rune")]
    public async Task<EnhanceRuneResponse> EnhanceRuneAsync([FromBody] EnhanceRuneRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EnhanceRune, "Request Enhance Rune", new { session.userId, request.runeId });
        
        var result = await _inventoryService.EnhanceRuneAsync(session.userId, request.runeId);
        return new EnhanceRuneResponse { code = result.ErrorCode };      
    }

    /// <summary>
    /// 캐릭터 강화 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 강화 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("character")]
    public async Task<EnhanceCharacterResponse> EnhanceCharacterAsync([FromBody] EnhanceCharacterRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.EnhanceCharacter, "Request Enhance Character", new { session.userId, request.characterId });

        var result = await _inventoryService.EnhanceCharacterAsync(session.userId, request.characterId);
        return new EnhanceCharacterResponse { code = result.ErrorCode };
    }
}