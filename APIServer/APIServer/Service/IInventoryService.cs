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
    Task<ErrorCode> EquipItem(long userId, long characterId, long itemId);

    /// <summary>
    /// 룬 장착 메서드
    /// 1) rune ID가 유요한지 확인
    /// 2) 이미 장착한 룬인지 확인
    /// 3) 룬 장착
    /// 
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<ErrorCode> EquipRune(long userId, long characterId,  long runeId);
}