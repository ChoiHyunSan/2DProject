using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Repository;
using static APIServer.ErrorCode;
using static APIServer.EventType;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class AccountService(ILogger<AccountService> logger, IAccountDb accountDb, IGameDb gameDb, IMemoryDb memoryDb) 
    : IAccountService
{
    private readonly ILogger<AccountService> _logger = logger;
    private readonly IAccountDb _accountDb = accountDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    public async Task<ErrorCode> RegisterAccountAsync(string email, string password)
    {
        var (_, isExists) = await _accountDb.CheckExistAccountByEmailAsync(email);
        if (isExists == false)
        {
            return DuplicatedEmail;
        }
        
        var (createResult, userId) = await CreateDefaultUserGameDataAsync();
        if (createResult == false)
        {
            await RollbackCreateDefaultUserGameDataAsync(userId);
            return FailedCreateUserData;       
        }
        
        if (await _accountDb.CreateAccountUserDataAsync(userId, email, password) != None)
        {
            await RollbackCreateDefaultUserGameDataAsync(userId);
            return FailedCreateAccount;      
        }
        
        return None;
    }
    
    public async Task<(GameData?, string, ErrorCode)> LoginAsync(string email, string password)
    {
        var (getUserAccountResult, account) = await _accountDb.GetUserAccountByEmail(email);
        if (getUserAccountResult != None)
        {
            return (null, "", getUserAccountResult);       
        }
        
        if (SecurityUtils.VerifyPassword(account.password, account.saltValue, password) == false)
        {
            return (null, "", FailedPasswordVerify);
        }
        
        var authToken = SecurityUtils.GenerateAuthToken();
        var (getAllGameDataResult, gameData) = await _gameDb.GetAllGameDataByUserIdAsync(account.userId);
        if (getAllGameDataResult != None)
        {
            return (null, "", FailedLoadUserData);
        }
        
        if (await _memoryDb.RegisterSessionAsync(CreateNewSession(account, authToken)) != None)
        {
            return (null, "", FailedRegisterSession);
        }

        if (await _memoryDb.CacheGameData(email, gameData) != None)
        {
            return (null, "", FailedCacheGameData);
        }
        
        return (gameData, authToken, None);
    }

    private static UserSession CreateNewSession(UserAccount account, string authToken)
    {
        return new UserSession
        {
            accountId = account.accountId,
            authToken = authToken,
            createDate = DateTime.Now,
            email = account.email,
            userId = account.userId
        };
    }

    // 기본 게임 데이터 생성 메서드 (유저 게임 데이터, 아이템, 룬, 출석, 퀘스트)
    private async Task<(bool, long)> CreateDefaultUserGameDataAsync()
    {
        LogInfo(_logger, CreateDefaultData, "Create Default User Data");
        
        var (result, userId) = await _gameDb.CreateUserGameDataAndReturnUserIdAsync();
        if (result != None || userId == 0 ||
            await CreateDefaultCharacterAsync(userId) != None ||
            await CreateDefaultItemAsync(userId) != None ||
            await CreateDefaultRuneAsync(userId) != None ||
            await CreateDefaultAttendanceAsync(userId) != None ||
            await CreateDefaultQuestAsync(userId) != None
            )
        {
            LogError(_logger, FailedInsertData, CreateDefaultData, "Failed Create Default User Data");
            return (false, userId);       
        }
        
        LogInfo(_logger, CreateDefaultData, "Success Create Default User Data", new { userId });
        
        return (true, userId);
    }

    private async Task<ErrorCode> CreateDefaultQuestAsync(long userId)
    {
        return await _gameDb.InsertQuestAsync(userId, 60000, DateTime.Now.AddYears(1));
    }

    private async Task<ErrorCode> CreateDefaultAttendanceAsync(long userId)
    {
        var result = await _gameDb.InsertAttendanceMonthAsync(userId);
        if (result != None)
        {
            return result;      
        }
        
        return await _gameDb.InsertAttendanceWeekAsync(userId);
    }

    private async Task<ErrorCode> CreateDefaultCharacterAsync(long userId)
    {
        var defaultCharacters = new[]
        {
            new UserInventoryCharacter { characterCode = 30001, level = 1 }
        };

        var result = None;
        foreach (var character in defaultCharacters)
        {
            result = await _gameDb.InsertCharacterAsync(userId, character);
        }

        return result;
    }
    
    private async Task<ErrorCode> CreateDefaultRuneAsync(long userId)
    {
        var defaultRunes = new[]
        {
            new UserInventoryRune { runeCode = 20000, level = 1 },
            new UserInventoryRune { runeCode = 20001, level = 1},
        };
        
        var result = None;
        foreach (var rune in defaultRunes)
        {
            result = await _gameDb.InsertRuneAsync(userId, rune);
        }
        return result;
    }

    private async Task<ErrorCode> CreateDefaultItemAsync(long userId)
    {
        var defaultItems = new[]
        {
            new UserInventoryItem { itemCode = 10001, level = 1 },
            new UserInventoryItem { itemCode = 10002, level = 1},
        };

        var result = None;
        foreach (var item in defaultItems)
        {
            result = await _gameDb.InsertItemAsync(userId, item);
        }

        return result;
    }

    // 기본 게임 데이터 롤백 메서드 
    private async Task RollbackCreateDefaultUserGameDataAsync(long userId)
    {
        LogInfo(_logger, RollBackDefaultData, "Start RollBack Default User Data");
        
        var result = await _gameDb.DeleteGameDataByUserIdAsync(userId);
        if (result != None)
        {
            LogError(_logger, FailedRollbackDefaultData, RollBackDefaultData, "Failed RollBack Default User Data", new { userId });
            return;
        }
        
        LogInfo(_logger, RollBackDefaultData, "Success RollBack Default User Data", new { userId });
    }
}