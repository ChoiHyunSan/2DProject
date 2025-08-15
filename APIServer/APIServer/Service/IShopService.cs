namespace APIServer.Service;

public interface IShopService
{
    /// <summary>
    /// 캐릭터 구매 메서드
    /// 1) 현재 재화 (골드, 잼) 보유 현황 조회
    /// 2) 구매 가능 여부 확인 (가격 & 보유 여부)
    /// 3) 구매가 가능한 경우, 구매 진행
    /// 4) 메모리에 올라가 있는 GameData 갱신
    ///
    /// 반환 값 : (에러 코드, 현재 골드 재화, 현재 유료 재화)
    /// </summary>
    Task<(ErrorCode errorCode, int currentGold, int currentGem)> PurchaseCharacter(long userId, long characterCode);

    /// <summary>
    /// 아이템 판매 메서드
    /// 
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<ErrorCode> SellItem(long userId, long itemId);
}