using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class AccountService(ILogger<AccountService> logger, IAccountDb accountDb, IGameDb gameDb, IMemoryDb memoryDb) 
    : IAccountService
{
    private readonly ILogger<AccountService> _logger = logger;
    private readonly IAccountDb _accountDb = accountDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    public async Task<Result> RegisterAccountAsync(string email, string password)
    {
        var (_, isExists) = await _accountDb.CheckExistAccountByEmailAsync(email);
        if (isExists)
        {
            return ErrorCode.DuplicatedEmail;
        }
        
        var (createResult, userId) = await CreateDefaultUserGameDataAsync();
        if (createResult == false)
        {
            await RollbackCreateDefaultUserGameDataAsync(userId);
            return ErrorCode.FailedCreateUserData;       
        }
        
        if (await _accountDb.CreateAccountUserDataAsync(userId, email, password) != ErrorCode.None)
        {
            await RollbackCreateDefaultUserGameDataAsync(userId);
            return ErrorCode.FailedCreateAccount;      
        }
        
        return ErrorCode.None;
    }
    
    public async Task<Result<(GameData, string)>> LoginAsync(string email, string password)
    {
        var (getUserAccountResult, account) = await _accountDb.GetUserAccountByEmail(email);
        if (getUserAccountResult != ErrorCode.None)
        {
            return Result<(GameData, string)>.Failure(getUserAccountResult);
        }
        
        if (SecurityUtils.VerifyPassword(account.password, account.saltValue, password) == false)
        {
            return Result<(GameData, string)>.Failure(ErrorCode.FailedPasswordVerify);
        }
        
        var authToken = SecurityUtils.GenerateAuthToken();
        var (getAllGameDataResult, gameData) = await _gameDb.GetAllGameDataByUserIdAsync(account.userId);
        if (getAllGameDataResult != ErrorCode.None)
        {
            return Result<(GameData, string)>.Failure(ErrorCode.FailedLoadUserData);
        }
        
        if (await _memoryDb.RegisterSessionAsync(CreateNewSession(account, authToken)) != ErrorCode.None)
        {
            return Result<(GameData, string)>.Failure(ErrorCode.FailedRegisterSession);
        }

        if (await _memoryDb.CacheGameData(email, gameData) != ErrorCode.None)
        {
            return Result<(GameData, string)>.Failure(ErrorCode.FailedCacheGameData);
        }
        
        return Result<(GameData, string)>.Success((gameData, authToken));
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
        LogInfo(_logger, EventType.CreateDefaultData, "Create Default User Data");
        
        var (result, userId) = await _gameDb.CreateUserGameDataAndReturnUserIdAsync();
        if (result != ErrorCode.None || userId == 0 ||
            await CreateDefaultCharacterAsync(userId) != ErrorCode.None ||
            await CreateDefaultItemAsync(userId) != ErrorCode.None ||
            await CreateDefaultRuneAsync(userId) != ErrorCode.None ||
            await CreateDefaultAttendanceAsync(userId) != ErrorCode.None ||
            await CreateDefaultQuestAsync(userId) != ErrorCode.None
            )
        {
            LogError(_logger, ErrorCode.FailedInsertData, EventType.CreateDefaultData, "Failed Create Default User Data");
            return (false, userId);       
        }
        
        LogInfo(_logger, EventType.CreateDefaultData, "Success Create Default User Data", new { userId });
        
        return (true, userId);
    }

    private async Task<ErrorCode> CreateDefaultQuestAsync(long userId)
    {
        var result =  await _gameDb.InsertQuestAsync(userId, 60000, DateTime.Now.AddYears(1));
        return result.ErrorCode;
    }

    private async Task<ErrorCode> CreateDefaultAttendanceAsync(long userId)
    {
        var result = await _gameDb.InsertAttendanceMonthAsync(userId);
        if (result != ErrorCode.None)
        {
            return result.ErrorCode;      
        }
        
        result = await _gameDb.InsertAttendanceWeekAsync(userId);
        return result.ErrorCode;
    }

    private async Task<ErrorCode> CreateDefaultCharacterAsync(long userId)
    {
        var defaultCharacters = new[]
        {
            new UserInventoryCharacter { characterCode = 30001, level = 1 }
        };
        
        foreach (var character in defaultCharacters)
        {
            var result = await _gameDb.InsertCharacterAsync(userId, character);
            if (!result.IsSuccess)
            {
                return result.ErrorCode;
            }
        }

        return ErrorCode.None;
    }
    
    private async Task<ErrorCode> CreateDefaultRuneAsync(long userId)
    {
        var defaultRunes = new[]
        {
            new UserInventoryRune { runeCode = 20000, level = 1 },
            new UserInventoryRune { runeCode = 20001, level = 1},
        };
        
        foreach (var rune in defaultRunes)
        {
            var result = await _gameDb.InsertRuneAsync(userId, rune);
            if (!result.IsSuccess)
            {
                return result.ErrorCode;
            }
        }
        return ErrorCode.None;
    }

    private async Task<ErrorCode> CreateDefaultItemAsync(long userId)
    {
        var defaultItems = new[]
        {
            new UserInventoryItem { itemCode = 10001, level = 1 },
            new UserInventoryItem { itemCode = 10002, level = 1},
        };
        
        foreach (var item in defaultItems)
        {
            var result = await _gameDb.InsertItemAsync(userId, item);
            if (!result.IsSuccess)
            {
                return result.ErrorCode;
            }
        }

        return ErrorCode.None;
    }

    // 기본 게임 데이터 롤백 메서드 
    private async Task RollbackCreateDefaultUserGameDataAsync(long userId)
    {
        LogInfo(_logger, EventType.RollBackDefaultData, "Start RollBack Default User Data");
        
        var result = await _gameDb.DeleteGameDataByUserIdAsync(userId);
        if (result != ErrorCode.None)
        {
            LogError(_logger, ErrorCode.FailedRollbackDefaultData, EventType.RollBackDefaultData, "Failed RollBack Default User Data", new { userId });
            return;
        }
        
        LogInfo(_logger, EventType.RollBackDefaultData, "Success RollBack Default User Data", new { userId });
    }
}