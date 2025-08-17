using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellController(ILogger<SellController> logger, IShopService shopService)
    : ControllerBase
{
    private readonly ILogger<SellController> _logger = logger;
    private readonly IShopService _shopService = shopService;

    [HttpPost]
    [Route("item")]
    public async Task<ItemSellResponse> SellItemAsync([FromBody] ItemSellRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LoggerManager.LogInfo(_logger, EventType.SellItem, "Request Sell Item", new { session.userId, request.itemId });
        
        var errorCode = await _shopService.SellItemAsync(session.userId, request.itemId);
        if (errorCode != ErrorCode.None)
        {
            return new ItemSellResponse { code = errorCode };       
        }
        
        LoggerManager.LogInfo(_logger, EventType.SellItem, "Sell Item Success", new { request, errorCode });
        return new ItemSellResponse { code = errorCode };       
    }
}