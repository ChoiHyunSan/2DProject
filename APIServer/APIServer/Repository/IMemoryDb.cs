using System.ComponentModel.DataAnnotations;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Redis;

namespace APIServer.Repository;

public interface IMemoryDb
{
    /// <summary> 세션 저장 </summary>
    Task<bool> RegisterSessionAsync(UserSession session);
    
    /// <summary> 이메일 기반 세션 조회 </summary>
    Task<Result<UserSession>> GetSessionByEmail(string email);

    /// <summary> 세션 Lock </summary>
    Task<Result> TrySessionRequestLock(long userId, TimeSpan? ttl = null);
    
    /// <summary> 세션 UnLock </summary>
    Task<Result> TrySessionRequestUnLock(long userId);

    /// <summary> 게임 데이터 캐싱 </summary>
    Task<bool> CacheGameData(long userId, GameData gameData);

    /// <summary> 스테이지 인게임 정보 캐싱 </summary>
    Task<bool> CacheStageInfo(InStageInfo inStageInfo);

    /// <summary> 스테이지 인게임 정보 조회 </summary>
    Task<Result<InStageInfo>> GetGameInfo(long userId);

    Task<bool> DeleteStageInfo(InStageInfo stageInfo);
    
    Task<Result<List<UserQuestInprogress>>> GetCachedQuestList(long userId);
    Task<Result> CacheQuestList(long userId, List<UserQuestInprogress> progressList);
}