using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController(ILogger<PurchaseController> logger, IShopService shopService)
    : ControllerBase
{
    private readonly ILogger<PurchaseController> _logger = logger;
    private readonly IShopService _shopService = shopService;
    
    /// <summary>
    /// 캐릭터 구매 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 장착 요청 결과 (성공 : ErrorCode.None)
    /// - 구매 결과 (캐릭터 코드, 남은 골드, 남은 유료 재화)
    /// </summary>
    [HttpPost]
    [Route("character")]
    public async Task<PurchaseCharacterResponse> PurchaseCharacterAsync([FromBody] PurchaseCharacterRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.PurchaseCharacter, "Request Purchase Character", new { session.userId, request.characterCode });
        
        var (errorCode, currentGold, currentGem) = await _shopService.PurchaseCharacterAsync(session.userId, request.characterCode);
        if (errorCode != ErrorCode.None)
        {
            return new PurchaseCharacterResponse { code = errorCode };       
        }
        
        LogInfo(_logger, EventType.PurchaseCharacter, "Purchase Character Success", new { request, currentGold, currentGem });
        
        return new PurchaseCharacterResponse
        {
            characterCode = request.characterCode,
            currentGold = currentGold,
            currentGem = currentGem,
        };
    }
}