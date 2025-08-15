using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController(ILogger<PurchaseController> logger, IShopService shopService)
    : ControllerBase
{
    private readonly ILogger<PurchaseController> _logger = logger;
    private readonly IShopService _shopService = shopService;
    
    [HttpPost]
    [Route("character")]
    public async Task<PurchaseCharacterResponse> PurchaseCharacterAsync([FromBody] PurchaseCharacterRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LoggerManager.LogInfo(_logger, EventType.PurchaseCharacter, "Request Purchase Character", new { session.userId, request.characterCode });
        
        var (errorCode, currentGold, currentGem) = await _shopService.PurchaseCharacter(session.userId, request.characterCode);
        if (errorCode != ErrorCode.None)
        {
            return new PurchaseCharacterResponse { code = errorCode };       
        }
        
        LoggerManager.LogInfo(_logger, EventType.PurchaseCharacter, "Purchase Character Success", new { request, currentGold, currentGem });
        
        return new PurchaseCharacterResponse
        {
            characterCode = request.characterCode,
            currentGold = currentGold,
            currentGem = currentGem,
        };
    }
}