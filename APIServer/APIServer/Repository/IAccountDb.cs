using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IAccountDb
{
    Task<bool> CheckExistAccountByEmailAsync(string email);
    Task<bool> CreateAccountUserDataAsync(long userId, string email, string password);
    Task<UserAccount> GetUserAccountByEmail(string email);
}