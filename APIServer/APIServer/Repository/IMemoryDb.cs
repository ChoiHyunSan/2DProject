using System.ComponentModel.DataAnnotations;
using APIServer.Models.DTO;
using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IMemoryDb
{
    /// <summary>
    /// 세션 정보 저장 메서드
    /// - Email 값을 이용한 키를 사용하여 세션 정보를 저장한다.
    /// 
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    Task<Result> RegisterSessionAsync(UserSession session);
    
    /// <summary>
    /// 이메일 기반 세션 조회 메서드
    ///
    /// 반환 값 : (에러 코드, UserSession 객체) (성공 : ErrorCode.None)
    /// </summary>
    Task<Result<UserSession>> GetSessionByEmail(string email);

    /// <summary>
    /// 세션 Lock 메서드
    ///
    /// 반환 값 : 에러 코드
    /// </summary>
    Task<Result> TrySessionRequestLock(string email, TimeSpan? ttl = null);
    
    /// <summary>
    /// 세션 UnLock 메서드
    ///
    /// 반환 값 : 에러 코드
    /// </summary>
    Task<Result> TrySessionRequestUnLock(string email);

    /// <summary>
    /// 게임 데이터 캐싱 메서드
    ///
    /// 반환 값 : 에러 코드
    /// </summary>
    Task<Result> CacheGameData(string email, GameData gameData);
}