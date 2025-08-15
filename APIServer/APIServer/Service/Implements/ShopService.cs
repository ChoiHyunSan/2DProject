using APIServer.Repository;

namespace APIServer.Service.Implements;

public class ShopService(ILogger<ShopService> logger, IMasterDb masterDb, IGameDb gameDb, IMemoryDb memoryDb)
    : IShopService
{
    private readonly ILogger<ShopService> _logger = logger;
    private readonly IMasterDb _masterDb = masterDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    public async Task<(ErrorCode errorCode, int currentGold, int currentGem)> PurchaseCharacter(long userId, long characterCode)
    {
        var (getDataResult, originData) = await _masterDb.GetCharacterOriginDataAsync(characterCode);
        if (getDataResult != ErrorCode.None)
        {
            return (getDataResult, 0, 0);
        }
        
        var (purchaseResult, currentGold, currentGem) = await _gameDb.PurchaseCharacter(userId, characterCode, originData.priceGold , originData.priceGem);
        if (purchaseResult != ErrorCode.None)
        {
            return (purchaseResult, 0, 0);
        }
        
        LoggerManager.LogInfo(_logger, EventType.PurchaseCharacter, "Purchase Character Success", new { userId, characterCode, originData.priceGold, originData.priceGem });
        
        return (ErrorCode.None, currentGold, currentGem); 
    }

    public async Task<ErrorCode> SellItem(long userId, long itemId)
    {
        LoggerManager.LogInfo(_logger, EventType.SellItem, "Sell Item", new { userId, itemId });
        
        return await _gameDb.SellInventoryItem(userId, itemId);
    }
}