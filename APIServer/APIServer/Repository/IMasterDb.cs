using APIServer.Models.Entity.Data;

namespace APIServer.Repository;

public interface IMasterDb
{
    /// <summary>
    /// Master Db 데이터를 메모리로 로드하는 메서드
    ///
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    public Task<ErrorCode> Load();

    /// <summary>
    /// 캐릭터 원본 데이터 조회 메서드
    /// 
    /// 반환 값 : (에러 코드, 원본 데이터)
    /// </summary>
    Task<(ErrorCode, CharacterOriginData)> GetCharacterOriginDataAsync(long characterCode);

    /// <summary>
    /// 아이템 판매 가격 조회 메서드
    ///
    /// 반환 값 : (에러 코드, 판매 가격)
    /// </summary>
    Task<(ErrorCode, int)> GetItemSellPriceAsync(long itemCode, int level);

    /// <summary>
    /// 아이템 강화 정보 조회 메서드
    ///
    /// 반환 값 : (
    /// </summary>
    Task<(ErrorCode, ItemEnhanceData)> GetItemEnhanceData(long itemCode, int level);
    
    /// <summary>
    ///
    /// 
    /// </summary>
    Task<(ErrorCode, RuneEnhanceData)> GetRuneEnhanceData(long runeCode, int level);
}