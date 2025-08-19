namespace APIServer.Service;

public interface IInventoryService
{
    /// <summary> 아이템 장착 </summary>
    Task<Result> EquipItemAsync(long userId, long characterId, long itemId);

    /// <summary> 룬 장착 </summary>
    Task<Result> EquipRuneAsnyc(long userId, long characterId,  long runeId);

    /// <summary> 아이템 강화 </summary>
    Task<Result> EnhanceItemAsync(long userId, long itemId);
    
    /// <summary> 룬 강화 </summary>
    Task<Result> EnhanceRuneAsync(long userId, long runeId);
}