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

    /// <summary>
    /// 아이템 판매 요청 API
    /// 세션 인증 : O
    /// 반환 값 :
    /// - 반환 코드 : 강화 요청 결과 (성공 : ErrorCode.None)
    /// </summary>
    [HttpPost]
    [Route("item")]
    public async Task<ItemSellResponse> SellItemAsync([FromBody] ItemSellRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LoggerManager.LogInfo(_logger, EventType.SellItem, "Request Sell Item", new { session.userId, request.itemId });
        
        var result = await _shopService.SellItemAsync(session.userId, request.itemId);
        return new ItemSellResponse { code = result.ErrorCode };       
    }
}