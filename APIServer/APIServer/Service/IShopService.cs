namespace APIServer.Service;

public interface IShopService
{
    /// <summary> 캐릭터 구매 </summary>
    Task<Result<(int currentGold, int currentGem)>> PurchaseCharacterAsync(long userId, long characterCode);

    /// <summary> 아이템 판매 </summary>
    Task<Result> SellItemAsync(long userId, long itemId);
}