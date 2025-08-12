using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IAccountDb
{
    /// <summary>
    /// Email 값에 대한 계정이 존재여부 메서드
    ///
    /// 반환 값 : (에러 코드, 존재 여부)
    /// </summary>
    Task<(ErrorCode, bool)> CheckExistAccountByEmailAsync(string email);
    
    /// <summary>
    /// Account 계정 생성 요청 메서드
    ///
    /// 반환 값 : (에러 코드, 생성 여부)
    /// </summary>
    Task<ErrorCode> CreateAccountUserDataAsync(long userId, string email, string password);
    
    
    /// <summary>
    /// Email 값을 이용한 조회 메서드
    ///
    /// 반환 값 : (에러 코드, UserAccount 객체)
    /// </summary>
    Task<(ErrorCode, UserAccount)> GetUserAccountByEmail(string email);
}