using System.ComponentModel.DataAnnotations;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Redis;

namespace APIServer.Repository;

public interface IMemoryDb
{
    /// <summary> 세션 저장 </summary>
    Task<Result> RegisterSessionAsync(UserSession session);
    
    /// <summary> 이메일 기반 세션 조회 </summary>
    Task<Result<UserSession>> GetSessionByEmail(string email);

    /// <summary> 세션 Lock </summary>
    Task<Result> TrySessionRequestLock(string email, TimeSpan? ttl = null);
    
    /// <summary> 세션 UnLock </summary>
    Task<Result> TrySessionRequestUnLock(string email);

    /// <summary> 게임 데이터 캐싱 </summary>
    Task<Result> CacheGameData(string email, GameData gameData);

    /// <summary> 스테이지 인게임 정보 캐싱 </summary>
    Task<Result> CacheStageInfo(InStageInfo inStageInfo);

    /// <summary> 스테이지 인게임 정보 조회 </summary>
    Task<Result<InStageInfo>> GetGameInfo(string email);

    Task<Result> DeleteStageInfo(InStageInfo stageInfo);
}