using APIServer.Models.DTO;

namespace APIServer.Service;

public interface IAccountService
{
    /// <summary> 회원 가입 </summary>
    Task<Result> RegisterAccountAsync(string email, string password);
    
    /// <summary> 로그인 </summary>
    Task<Result<(long userId, string authToken)>> LoginAsync(string email, string password);
}