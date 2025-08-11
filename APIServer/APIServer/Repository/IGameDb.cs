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
    Task<long> CreateUserGameDataAndReturnUserIdAsync();
    
    /// <summary>
    /// 캐릭터 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<bool> InsertCharacterAsync(long userId, UserInventoryCharacter character);
    
    /// <summary>
    /// 아이템 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<bool> InsertItemAsync(long userId, UserInventoryItem item);
    
    /// <summary>
    /// 른 획득 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<bool> InsertRuneAsync(long userId, UserInventoryRune rune);
    
    /// <summary>
    /// 월간 출석 보상 데이터 생성 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<bool> InsertAttendanceMonthAsync(long userId);
    
    /// <summary>
    /// 주간 출석 보상 데이터 생성 메서드
    /// 
    /// 반환 값 : 획득 결과
    /// </summary>
    Task<bool> InsertAttendanceWeekAsync(long userId);
    
    /// <summary>
    /// 퀘스트 진행 데이터 생성 메서드
    /// 
    /// 반환 값 : 생성 결과
    /// </summary>
    Task<bool> InsertQuestAsync(long userId, long questCode, DateTime expireDate);
    
    /// <summary>
    /// 유저 게임 데이저 삭제 메서드
    /// 
    /// 반환 값 : 삭제 결과
    /// </summary>
    Task<bool> DeleteGameDataByUserIdAsync(long userId);

    /// <summary>
    /// 유저 전체 게임 데이터 조회 메서드
    ///
    /// 반환 값 : GameData
    /// </summary>
    Task<(bool, GameData?)> GetAllGameDataByUserIdAsync(long accountUserId);
}