using APIServer.Models.DTO;
using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IGameDb
{
    /// <summary> 유저 초기 데이터 생성 </summary>
    Task<Result<long>> CreateUserGameDataAndReturnUserIdAsync();
    
    /// <summary> 캐릭터 획득 </summary>
    Task<Result> InsertCharacterAsync(long userId, UserInventoryCharacter character);
    
    /// <summary> 아이템 획득 </summary>
    Task<Result> InsertItemAsync(long userId, UserInventoryItem item);
    
    /// <summary> 룬 획득 </summary>
    Task<Result> InsertRuneAsync(long userId, UserInventoryRune rune);
    
    /// <summary> 월간 출석 보상 생성 </summary>
    Task<Result> InsertAttendanceMonthAsync(long userId);
    
    /// <summary> 주간 출석 보상 생성 </summary>
    Task<Result> InsertAttendanceWeekAsync(long userId);
    
    /// <summary> 퀘스트 진행 정보 생성 </summary>
    Task<Result> InsertQuestAsync(long userId, long questCode, DateTime expireDate);
    
    /// <summary> 유저 게임 데이터 삭제 </summary>
    Task<Result> DeleteGameDataByUserIdAsync(long userId);

    /// <summary> 유저 전체 게임 데이터 조회 </summary>
    Task<Result<GameData>> GetAllGameDataByUserIdAsync(long accountUserId);

    /// <summary> 캐릭터 구매 </summary>
    Task<Result<(int currentGold, int currentGem)>> PurchaseCharacterAsync(long userId, long characterCode, int goldPrice, int gemPrice);

    /// <summary> 아이템 판매 </summary>
    Task<Result> SellInventoryItemAsync(long userId, long itemId);

    /// <summary> 아이템 장착 </summary>
    Task<Result> TryEquipItemAsync(long userId, long characterId, long itemId);
    
    /// <summary> 룬 장착 </summary>
    Task<Result> TryEquipRuneAsync(long userId, long characterId, long runeId);

    /// <summary> 아이템 강화 시도 </summary>
    Task<Result> TryEnhanceItemAsync(long userId, long itemId);
    
    /// <summary> 룬 강화 시도 </summary>
    Task<Result> TryEnhanceRuneAsync(long userId, long runeId);
}