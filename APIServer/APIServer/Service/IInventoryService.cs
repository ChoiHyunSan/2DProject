namespace APIServer.Service;

public interface IInventoryService
{
    /// <summary>
    /// 아이템 장착 메서드
    /// 1) item ID가 유효한지 확인
    /// 2) 이미 장착한 아이템인지 확인
    /// 3) 아이템 장착
    /// 
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<Result> EquipItemAsync(long userId, long characterId, long itemId);

    /// <summary>
    /// 룬 장착 메서드
    /// 1) rune ID가 유요한지 확인
    /// 2) 이미 장착한 룬인지 확인
    /// 3) 룬 장착
    /// 
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<Result> EquipRuneAsnyc(long userId, long characterId,  long runeId);

    /// <summary>
    /// 아이템 강화 메서드
    /// 1) item ID 유효성 검증
    /// 2) 강화가 가능한지 확인 (최대 강화레벨인 3인 경우 불가능, 재화 부족한 경우 불가능)
    /// 3) 강화가 가능한 경우 강화
    ///
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<Result> EnhanceItemAsync(long userId, long itemId);
    
    /// <summary>
    /// 룬 강화 메서드
    /// 1) rune ID 유효성 검증
    /// 2) 강화 가능 여부 확인 (레벨 3인 경우, 재화가 부족한 경우)
    /// 3) 강화가 가능한 경우 강화 진행
    ///
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<Result> EnhanceRuneAsync(long userId, long itemId);
}