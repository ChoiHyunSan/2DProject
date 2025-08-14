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
    Task<ErrorCode> RegisterSessionAsync(UserSession session);
    
    /// <summary>
    /// 이메일 기반 세션 조회 메서드
    ///
    /// 반환 값 : (에러 코드, UserSession 객체) (성공 : ErrorCode.None)
    /// </summary>
    Task<(ErrorCode, UserSession)> GetSessionByEmail(string email);

    Task<ErrorCode> TrySessionRequestLock(string email, TimeSpan? ttl = null);
    Task<ErrorCode> TrySessionRequestUnLock(string email);
}