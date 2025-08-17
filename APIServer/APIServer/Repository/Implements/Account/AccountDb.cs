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

    public async Task<Result<bool>> CheckExistAccountByEmailAsync(string email)
    {
        try
        {
            LogInfo(_logger, EventType.LoadAccountDb, "CheckExistsAccountByEmailAsync", new { email });
            
            var result =  await _queryFactory.Query("user_account")
                .SelectRaw("EXISTS(SELECT 1 FROM user_account WHERE email = ?)", email)
                .FirstOrDefaultAsync<bool>();

            return Result<bool>.Success(result);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadAccountDb, "CheckExistsAccountByEmailAsync Failed", new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return Result<bool>.Failure(ErrorCode.FailedDataLoad);
        }
    }

    public async Task<Result> CreateAccountUserDataAsync(long userId, string email, string password)
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
            
            LogInfo(_logger, EventType.CreateAccountUserData, "Create New Account", new { userId, email });
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedCreateAccountUserData, EventType.CreateAccountUserData, "Create New Account Failed", new
            {
                userId,
                email,
                e.Message,
                e.StackTrace
            });
            return ErrorCode.FailedCreateAccountUserData;
        }

        return ErrorCode.None;
    }

    public async Task<Result<UserAccount>> GetUserAccountByEmail(string email)
    {
        try
        {
            var userAccount = await _queryFactory.Query("user_account")
                .Where("email", email)
                .FirstAsync<UserAccount>();
            
            LogInfo(_logger, EventType.GetAccountUserData, "Get Account By Email", new { email });
            return Result<UserAccount>.Success(userAccount);
        }catch(Exception e)
        {
            LogError(_logger, ErrorCode.FailedGetAccountUserData, EventType.GetAccountUserData, "GetUserAccountByEmail Failed", new
            {
                email,
                e.Message,
                e.StackTrace
            });
            return Result<UserAccount>.Failure(ErrorCode.FailedGetAccountUserData);
        }
    }
}