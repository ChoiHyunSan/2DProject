using APIServer.Config;
using APIServer.Models.Entity;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static APIServer.ErrorCode;
using static APIServer.EventType;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

public class AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> dbConfig)
    : MySQLBase(dbConfig.Value.AccountDb), IAccountDb
{
    private readonly ILogger<AccountDb> _logger = logger;
    
    public async Task<(ErrorCode, bool)> CheckExistAccountByEmailAsync(string email)
    {
        try
        {
            LogInfo(_logger, LoadAccountDb, "CheckExistsAccountByEmailAsync", new { email });
            
            var result =  await _queryFactory.Query("user_account")
                .SelectRaw("EXISTS(SELECT 1 FROM user_account WHERE email = ?)", email)
                .FirstOrDefaultAsync<bool>();

            return (None, result);
        }
        catch (Exception e)
        {
            LogError(_logger, FailedDataLoad, LoadAccountDb, "CheckExistsAccountByEmailAsync Failed", new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return (FailedDataLoad, false);
        }
    }

    public async Task<ErrorCode> CreateAccountUserDataAsync(long userId, string email, string password)
    {
        try
        {
            var saltValue = SecurityUtils.GenerateSalt();
            var (_, hashPassword) = SecurityUtils.HashPassword(password, saltValue);
            
            _ = await _queryFactory.Query("user_account").InsertAsync( new 
            {
                user_id = userId,
                email = email,
                password = hashPassword,
                salt_value = saltValue,
            });
            
            LogInfo(_logger, CreateAccountUserData, "Create New Account", new { userId, email });
        }
        catch (Exception e)
        {
            LogError(_logger, FailedCreateAccountUserData, CreateAccountUserData, "Create New Account Failed", new
            {
                userId,
                email,
                e.Message,
                e.StackTrace
            });
            return FailedCreateAccountUserData;
        }

        return None;
    }

    public async Task<(ErrorCode, UserAccount)> GetUserAccountByEmail(string email)
    {
        try
        {
            var userAccount = await _queryFactory.Query("user_account")
                .Where("email", email)
                .FirstAsync<UserAccount>();
            
            LogInfo(_logger, GetAccountUserData, "Get Account By Email", new { email });
            return (None, userAccount);
        }catch(Exception e)
        {
            LogError(_logger, FailedGetAccountUserData, GetAccountUserData, "GetUserAccountByEmail Failed", new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return (FailedGetAccountUserData, new UserAccount());
        }
    }
}