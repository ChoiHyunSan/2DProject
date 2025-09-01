using APIServer.Config;
using APIServer.Models.Entity;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

public class AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> dbConfig)
    : MySQLBase(dbConfig.Value.AccountDb), IAccountDb
{
    private readonly ILogger<AccountDb> _logger = logger;

    public async Task<bool> CheckExistAccountByEmailAsync(string email)
    {
        return await _queryFactory.Query("user_account")
                .SelectRaw("EXISTS(SELECT 1 FROM user_account WHERE email = ?)", email)
                .FirstOrDefaultAsync<bool>();
    }

    public async Task<bool> CreateAccountUserDataAsync(long userId, string email, string saltValue, string password)
    {
        var result = await _queryFactory.Query("user_account").InsertAsync( new 
        {
            user_id = userId,
            email = email,
            password = password,
            salt_value = saltValue,
        });

        return result == 1;
    }

    public async Task<UserAccount> GetUserAccountByEmailAsync(string email)
    {
        return await _queryFactory.Query("user_account")
            .Where("email", email)
            .FirstOrDefaultAsync<UserAccount>();
    }
}