using System.Data;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Redis;
using SqlKata.Execution;

namespace APIServer.Repository;

public interface IGameDb
{
    /// <summary> 트랜잭션 코드 (반환형 X) </summary>
    Task<ErrorCode> WithTransactionAsync(Func<QueryFactory, Task<ErrorCode>> action);

    /// <summary> 트랜잭션 코드 (반환형 O) </summary>
    Task<TResult> WithTransactionAsync<TResult>(Func<QueryFactory, Task<TResult>> func);
    
    /// <summary> 유저 초기 데이터 생성 </summary>
    Task<long> CreateUserGameDataAndReturnUserIdAsync();
    
    /// <summary> 캐릭터 획득 </summary>
    Task<bool> InsertCharacterAsync(long userId, UserInventoryCharacter character);
    
    /// <summary> 아이템 획득 </summary>
    Task<bool> InsertItemAsync(long userId, UserInventoryItem item);
    
    /// <summary> 룬 획득 </summary>
    Task<bool> InsertRuneAsync(long userId, UserInventoryRune rune);
    
    /// <summary> 월간 출석 보상 생성 </summary>
    Task<bool> InsertAttendanceMonthAsync(long userId);
    
    /// <summary> 주간 출석 보상 생성 </summary>
    Task<bool> InsertAttendanceWeekAsync(long userId);
    
    /// <summary> 퀘스트 진행 정보 생성 </summary>
    Task<bool> InsertQuestAsync(long userId, long questCode, DateTime expireDate);
    
    /// <summary> 유저 게임 데이터 삭제 </summary>
    Task<bool> DeleteGameDataByUserIdAsync(long userId);

    /// <summary> 유저 전체 게임 데이터 조회 </summary>
    Task<GameData> GetAllGameDataByUserIdAsync(long accountUserId);

    /// <summary> 캐릭터 구매 </summary>
    Task<Result<(int gold, int gem)>> PurchaseCharacterAsync(long userId, long characterCode, int goldPrice, int gemPrice);

    /// <summary> 아이템 판매 </summary>
    Task<ErrorCode> SellInventoryItemAsync(long userId, long itemId);

    /// <summary> 아이템 장착 </summary>
    Task<ErrorCode> TryEquipItemAsync(long userId, long characterId, long itemId);
    
    /// <summary> 룬 장착 </summary>
    Task<ErrorCode> TryEquipRuneAsync(long userId, long characterId, long runeId);

    /// <summary> 아이템 강화 시도 </summary>
    Task<ErrorCode> TryEnhanceItemAsync(long userId, long itemId);
    
    /// <summary> 룬 강화 시도 </summary>
    Task<ErrorCode> TryEnhanceRuneAsync(long userId, long runeId);

    /// <summary> 클리어한 스테이지 목록 조회 </summary>
    Task<List<UserClearStage>> GetClearStageList(long userId);

    /// <summary> 스테이지 클리어 갱신 </summary>
    Task<bool> UpdateClearStageAsync(long userId, long stageCode);

    /// <summary> 스테이지 클리어 보상 제공 </summary>
    Task<bool> RewardClearStage(InStageInfo stageInfo);

    /// <summary> 유저 데이터 조회 </summary>
    Task<UserGameData> GetUserDataByEmailAsync(string email);
    
    /// <summary> 유저 데이터 갱신 </summary>
    Task<bool> UpdateUserDataAsync(UserGameData data);
}