using APIServer.Repository;

namespace APIServer.Service.Implements;

public class ShopService(ILogger<ShopService> logger, IMasterDb masterDb, IGameDb gameDb, IMemoryDb memoryDb)
    : IShopService
{
    private readonly ILogger<ShopService> _logger = logger;
    private readonly IMasterDb _masterDb = masterDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    public async Task<Result<(int currentGold, int currentGem)>> PurchaseCharacterAsync(long userId, long characterCode)
    {
        var originData = _masterDb.GetCharacterOriginDatas()[characterCode];
        var purchaseResult = await _gameDb.PurchaseCharacterAsync(userId, characterCode, originData.priceGold , originData.priceGem);
        if (!purchaseResult.IsSuccess)
        {
            return Result<(int, int)>.Failure(purchaseResult.ErrorCode);
        }
        
        LoggerManager.LogInfo(_logger, EventType.PurchaseCharacter, "Purchase Character Success", new { userId, characterCode, originData.priceGold, originData.priceGem });

        var (gold, gem) = purchaseResult.Value;
        return Result<(int,int)>.Success((gold, gem)); 
    }

    public async Task<Result> SellItemAsync(long userId, long itemId)
    {
        LoggerManager.LogInfo(_logger, EventType.SellItem, "Sell Item", new { userId, itemId });
        
        return await _gameDb.SellInventoryItemAsync(userId, itemId);
    }
}