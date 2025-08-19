using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IAccountDb
{
    /// <summary> 이메일 기반 계정 정보 유무 확인 </summary>
    Task<bool> CheckExistAccountByEmailAsync(string email);
    
    /// <summary> 계정 정보 생성 </summary>
    Task<bool> CreateAccountUserDataAsync(long userId, string email, string password);
    
    
    /// <summary> 이메일 기반 계정 정보 조회 </summary>
    Task<UserAccount> GetUserAccountByEmailAsync(string email);
}