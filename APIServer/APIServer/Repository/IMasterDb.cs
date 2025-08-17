using APIServer.Models.Entity.Data;

namespace APIServer.Repository;

public interface IMasterDb
{
    /// <summary> Master Db 데이터 로드 </summary>
    public Task<ErrorCode> Load();
    
    /// <summary> 캐릭터 원본 데이터 조회 </summary>
    Task<Result<CharacterOriginData>> GetCharacterOriginDataAsync(long characterCode);
    
    /// <summary> 아이템 판매 가격 조회 </summary>
    Task<Result<int>> GetItemSellPriceAsync(long itemCode, int level);

    /// <summary> 아이템 강화 정보 조회 </summary>
    Task<Result<ItemEnhanceData>> GetItemEnhanceData(long itemCode, int level);
    
    /// <summary> 룬 강화 정보 조회 </summary>
    Task<Result<RuneEnhanceData>> GetRuneEnhanceData(long runeCode, int level);
}
