using APIServer.Models.DTO;
using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IGameDb
{
    Task<UserGameData> TestInsert();
    
    /// <summary>
    /// 유저 데이터 생성 메서드
    /// - 초기 데이터 값을 데이터화 하지 않고 하드 코딩으로 작성
    /// 
    /// 반환 값 : 생성된 유저 ID (실패 시, 0 반환)
    /// </summary>
    Task<(ErrorCode, long)> CreateUserGameDataAndReturnUserIdAsync();
    
    /// <summary>
    /// 캐릭터 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<ErrorCode> InsertCharacterAsync(long userId, UserInventoryCharacter character);
    
    /// <summary>
    /// 아이템 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<ErrorCode> InsertItemAsync(long userId, UserInventoryItem item);
    
    /// <summary>
    /// 른 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<ErrorCode> InsertRuneAsync(long userId, UserInventoryRune rune);
    
    /// <summary>
    /// 월간 출석 보상 데이터 생성 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<ErrorCode> InsertAttendanceMonthAsync(long userId);
    
    /// <summary>
    /// 주간 출석 보상 데이터 생성 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<ErrorCode> InsertAttendanceWeekAsync(long userId);
    
    /// <summary>
    /// 퀘스트 진행 데이터 생성 메서드
    /// 
    /// 반환 값 : 생성 결과
    /// </summary>
    Task<ErrorCode> InsertQuestAsync(long userId, long questCode, DateTime expireDate);
    
    /// <summary>
    /// 유저 게임 데이저 삭제 메서드
    /// 
    /// 반환 값 : 삭제 결과
    /// </summary>
    Task<ErrorCode> DeleteGameDataByUserIdAsync(long userId);

    /// <summary>
    /// 유저 전체 게임 데이터 조회 메서드
    ///
    /// 반환 값 : GameData
    /// </summary>
    Task<(ErrorCode, GameData?)> GetAllGameDataByUserIdAsync(long accountUserId);

    /// <summary>
    /// 캐릭터 구매 요청 메서드
    /// 1) 현재 UserData의 Gold, Gem 조회
    /// 2) 구매할 수 있는지 비교
    /// 3) 구매 가능한 경우, 재화를 차감하여 캐릭터 구매
    /// 4) 계산 결과 DB에 반영
    ///
    /// - 3 ~ 4번은 하나의 트랜잭션으로 묶인 상태로 작업한다. 
    /// 반환 값 : (구매 결과 에러 코드, 현재 남은 골드 재화, 현재 남은 유료 재화)
    /// </summary>
    Task<(ErrorCode errorCode, int currentGold, int currentGem)> PurchaseCharacter(long userId, long characterCode, int goldPrice, int gemPrice);

    /// <summary>
    /// 아이템 판매 메서드
    /// 1) 아이템을 가지고 있다면 삭제
    /// 2) 아이템 가격에 맞는 판매 가격만큼 재화 획득
    ///
    /// - 장착중인 아이템은 판매할 수 없다.
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<ErrorCode> SellInventoryItem(long userId, long itemId);

    /// <summary>
    /// 
    /// 
    /// </summary>
    Task<ErrorCode> TryEquipItem(long userId, long characterId, long itemId);
    
    /// <summary>
    /// 
    /// 
    /// </summary>
    Task<ErrorCode> TryEquipRune(long userId, long characterId, long runeId);
}