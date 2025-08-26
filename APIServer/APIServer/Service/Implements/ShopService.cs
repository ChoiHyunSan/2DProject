using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using static APIServer.LoggerManager;

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
        try
        {
            var originData = _masterDb.GetCharacterOriginDatas()[characterCode];
            var (gold, gem) = (priceGold: originData.price_gold, priceGem: originData.price_gem);

            // 보유 여부 확인
            if (await _gameDb.CheckAlreadyHaveCharacterAsync(userId, characterCode) == false)
            {
                return Result<(int, int)>.Failure(ErrorCode.CannotFindCharacter);
            }

            // 유저 재화 조회
            var (currentGold, currentGem) = await _gameDb.GetUserCurrencyAsync(userId);

            // 구매 가능 여부 확인
            if (VerifyPurchase(currentGold, currentGem, gold, gem) == false)
            {
                return Result<(int gold, int gem)>.Failure(ErrorCode.CannotPurchaseCharacter);
            }

            // 결과 값 계산
            var (newGold, newGem) = CalculatePurchaseNewCurrency(currentGold, currentGem, gold, gem);

            // 트랜잭션 처리
            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 재화 차감
                if (await _gameDb.UpdateUserCurrencyAsync(userId, newGold, newGem) == false)
                {
                    return ErrorCode.FailedUpdateData;
                }

                // 캐릭터 추가
                if (await _gameDb.InsertNewCharacterAsync(userId, characterCode) == false)
                {
                    return ErrorCode.FailedInsertNewCharacter;
                }

                // 캐시 삭제
                if (await _memoryDb.DeleteCacheData(userId, [CacheType.Character, CacheType.UserGameData]) is var deleteCache && deleteCache.IsFailed)
                {
                    return deleteCache.ErrorCode;
                }
                
                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return Result<(int gold, int gem)>.Failure(txErrorCode);
            }

            LogInfo(_logger, EventType.PurchaseCharacter, "Purchase Character Success",
                new { userId, characterCode, gold, gem });

            return Result<(int, int)>.Success((newGold, newGem));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedPurchaseCharacter, EventType.PurchaseCharacter, 
                "Failed Purchase Character", new { userId, characterCode, ex.Message, ex.StackTrace });
            return Result<(int, int)>.Failure(ErrorCode.FailedPurchaseCharacter);;
        }
    }

    public async Task<Result> SellItemAsync(long userId, long itemId)
    {
        try
        {
            // 아이템 조회
            var item = await _gameDb.GetInventoryItemAsync(userId, itemId);

            // 장착 여부 확인
            if (await _gameDb.IsItemEquippedAsync(itemId))
            {
                return ErrorCode.CannotSellEquipmentItem;
            }

            // 판매가 및 현재 잔액 조회
            var sellGold = _masterDb.GetItemEnhanceDatas()[(itemId, item.level)].sell_price;
            var (curGold, curGem) = await _gameDb.GetUserCurrencyAsync(userId);

            // 결과 잔액 계산 
            var (newGold, newGem) = CalculateSellNewCurrency(curGold, curGem, sellGold);

            // 트랜잭션
            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 아이템 삭제
                if (await _gameDb.DeleteInventoryItemAsync(userId, itemId) == false)
                {
                    return ErrorCode.FailedDeleteInventoryItem;
                }

                // 재화 갱신
                if (await _gameDb.UpdateUserCurrencyAsync(userId, newGold, newGem) == false)
                {
                    return ErrorCode.FailedUpdateUserGoldAndGem;
                }

                // 캐시 삭제
                if (await _memoryDb.DeleteCacheData(userId, [CacheType.Item, CacheType.UserGameData]) is var deleteCache && deleteCache.IsFailed)
                {
                    return deleteCache.ErrorCode;
                }
                
                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return Result.Failure(txErrorCode);
            }

            LogInfo(_logger, EventType.SellItem, "Sell Item", new { userId, itemId });

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedSellItem, EventType.SellItem,
                "Failed Sell Item", new { userId, itemId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedSellItem);
        }
    }

    private bool VerifyPurchase(int gold, int gem, int priceGold, int priceGem)
    {
        return gold >= priceGold && gem >= priceGem;
    }

    private (int newGold, int newGem) CalculatePurchaseNewCurrency(int currentGold, int currentGem, int priceGold, int priceGem)
    {
        return (currentGold - priceGold, currentGem - priceGem);
    }

    private (int newGold, int newGem) CalculateSellNewCurrency(int currentGold, int currentGem, int sellPrice, int sellGem = 0)
    {
        return (currentGold + sellPrice, currentGem + sellGem);
    }
}