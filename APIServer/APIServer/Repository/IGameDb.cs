using System.Data;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
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

    /// <summary> 클리어한 스테이지 목록 조회 </summary>
    Task<List<UserClearStage>> GetClearStageListAsync(long userId);

    /// <summary> 스테이지 클리어 갱신 </summary>
    Task<bool> UpdateClearStageAsync(long userId, long stageCode);

    /// <summary> 스테이지 클리어 보상 제공 </summary>
    Task<bool> RewardClearStageAsync(InStageInfo stageInfo);

    /// <summary> 유저 데이터 조회 </summary>
    Task<UserGameData> GetUserDataByEmailAsync(string email);
    
    /// <summary> 유저 데이터 갱신 </summary>
    Task<bool> UpdateUserDataAsync(UserGameData data);

    /// <summary> 캐릭터 ID 기반 보유 여부 조회 </summary>
    Task<bool> IsCharacterExistsAsync(long userId, long characterId);

    /// <summary> 룬 장착 여부 조회 </summary>
    Task<bool> IsRuneEquippedAsync(long runeId);

    /// <summary> 아이템 보유 여부 조회 </summary>
    Task<bool> IsItemExistsAsync(long userId, long itemId);

    /// <summary> 룬 보유 여부 조회 </summary>
    Task<bool> IsRuneExistsAsync(long userId, long runeId);

    /// <summary> 아이템 장착 정보 생성 </summary>
    Task<bool> EquipItemAsync(long characterId, long itemId);

    /// <summary> 룬 장착 정보 생성 </summary>
    Task<bool> EquipRuneAsync(long characterId, long runeId);

    /// <summary> 룬 정보 갱신 </summary>
    Task<bool> UpdateRuneLevelAsync(long userId, long runeId, int newLevel);

    /// <summary> 아이템 정보 갱신 </summary>
    Task<bool> UpdateItemLevelAsync(long userId, long itemId, int newLevel);

    /// <summary> 인벤토리 아이템 조회 </summary>
    Task<UserInventoryItem> GetInventoryItemAsync(long userId, long itemId);

    /// <summary> 인벤토리 룬 조회 </summary>
    Task<UserInventoryRune> GetInventoryRuneAsync(long userId, long runeId);

    /// <summary> 아이템 장착 여부 조회 </summary>
    Task<bool> IsItemEquippedAsync(long itemId);

    /// <summary> 인벤토리 아이템 삭제 </summary>
    Task<bool> DeleteInventoryItemAsync(long userId, long itemId);

    /// <summary> 유저 현재 재화 조회 </summary>
    Task<(int gold, int gem)> GetUserCurrencyAsync(long userId);

    /// <summary> 유저 재화 갱신 </summary>
    Task<bool> UpdateUserCurrencyAsync(long userId, int gold, int gem);

    /// <summary> 캐릭터 보유 여부 조회 </summary>
    Task<bool> CheckAlreadyHaveCharacterAsync(long userId, long characterCode);

    /// <summary> 인벤토리 캐릭터 추가 </summary>
    Task<bool> InsertNewCharacterAsync(long userId, long characterCode);

    /// <summary> 스테이지 클리어 정보 조회 </summary>
    Task<UserClearStage> FindClearStageAsync(long userId, long stageCode);

    /// <summary> 스테이지 클리어 정보 추가 </summary>
    Task<bool> InsertClearStageAsync(long userId, long stageCode);

    /// <summary> 스테이지 클리어 정보 갱신 </summary>
    Task<bool> UpdateStageAsync(UserClearStage current);

    /// <summary> 유저 골드 갱신 </summary>
    Task<bool> UpdateUserGoldAsync(long userId, int newGold);

    /// <summary> 드랍 아이템 동시 추가 </summary>
    Task<bool> InsertDropItems(long userId, List<StageRewardItem> dropItems);
    
    /// <summary> 드랍 룬 동시 추가 </summary>
    Task<bool> InsertDropRunes(long userId, List<StageRewardRune> dropRunes);

    Task<UserAttendanceMonth> GetUserAttendance(long userId);
    
    Task<bool> UpdateAttendanceToday(long userId, int day);
    Task<bool> UpdateUserGemAsync(long userId, int price);
}