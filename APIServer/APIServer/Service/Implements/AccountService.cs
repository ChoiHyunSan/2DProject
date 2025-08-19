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
        // 중복 이메일 조회
        if (await _accountDb.CheckExistAccountByEmailAsync(email))
        {
            return ErrorCode.DuplicatedEmail;    
        }
        
        // 기본 유저 게임데이터 생성
        if(await CreateDefaultUserGameDataAsync() is var created && created.IsFailed)
        {
            return created.ErrorCode;
        }
        var userId = created.Value;

        // 계정 정보 생성
        if(await _accountDb.CreateAccountUserDataAsync(userId, email, password) == false)
        {
            await RollbackCreateDefaultUserGameDataAsync(userId);
        }

        return ErrorCode.None;
    }
    
    public async Task<Result<(long, string)>> LoginAsync(string email, string password)
    {
        // 계정 조회
        var account = await _accountDb.GetUserAccountByEmailAsync(email);

        // 패스워드 검증
        if (!SecurityUtils.VerifyPassword(account.password, account.saltValue, password))
        {
            return Result<(long, string)>.Failure(ErrorCode.FailedPasswordVerify);   
        }

        // 토큰 발급 + 세션 등록
        var token = SecurityUtils.GenerateAuthToken();
        if (await _memoryDb.RegisterSessionAsync(CreateNewSession(account, token)) == false)
        {
            return Result<(long, string)>.Failure(ErrorCode.FailedRegisterSession);
        }

        return Result<(long, string)>.Success((account.userId, token));
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
    private async Task<Result<long>> CreateDefaultUserGameDataAsync()
    {
        LogInfo(_logger, EventType.CreateDefaultData, "Create Default User Data");
        
        var userId = await _gameDb.CreateUserGameDataAndReturnUserIdAsync();
        
        if (userId == 0 ||
            await CreateDefaultCharacterAsync(userId) == false ||
            await CreateDefaultItemAsync(userId) == false ||
            await CreateDefaultRuneAsync(userId) == false ||
            await CreateDefaultAttendanceAsync(userId) == false ||
            await CreateDefaultQuestAsync(userId) == false
            )
        {
            LogError(_logger, ErrorCode.FailedInsertData, EventType.CreateDefaultData, "Failed Create Default User Data");
            return Result<long>.Failure(ErrorCode.FailedInsertData, userId);     
        }
        
        LogInfo(_logger, EventType.CreateDefaultData, "Success Create Default User Data", new { userId });
        
        return Result<long>.Success(userId);
    }

    private async Task<bool> CreateDefaultQuestAsync(long userId)
    {
        return await _gameDb.InsertQuestAsync(userId, 60000, DateTime.Now.AddYears(1));
    }

    private async Task<bool> CreateDefaultAttendanceAsync(long userId)
    {
        return await _gameDb.InsertAttendanceMonthAsync(userId) && await _gameDb.InsertAttendanceWeekAsync(userId);
    }

    private async Task<bool> CreateDefaultCharacterAsync(long userId)
    {
        var defaultCharacters = new[]
        {
            new UserInventoryCharacter { characterCode = 30001, level = 1 }
        };
        
        foreach (var character in defaultCharacters)
        {
            var result = await _gameDb.InsertNewCharacterAsync(userId, character.characterCode);
            if (result == false)
            {
                return false;
            }
        }

        return true;
    }
    
    private async Task<bool> CreateDefaultRuneAsync(long userId)
    {
        var defaultRunes = new[]
        {
            new UserInventoryRune { runeCode = 20000, level = 1 },
            new UserInventoryRune { runeCode = 20001, level = 1},
        };
        
        foreach (var rune in defaultRunes)
        {
            var result = await _gameDb.InsertRuneAsync(userId, rune);
            if (result == false)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> CreateDefaultItemAsync(long userId)
    {
        var defaultItems = new[]
        {
            new UserInventoryItem { itemCode = 10001, level = 1 },
            new UserInventoryItem { itemCode = 10002, level = 1},
        };
        
        foreach (var item in defaultItems)
        {
            var result = await _gameDb.InsertItemAsync(userId, item);
            if (result == false)
            {
                return false;
            }
        }

        return true;
    }
    
    private async Task RollbackCreateDefaultUserGameDataAsync(long userId)
    {
        LogInfo(_logger, EventType.RollBackDefaultData, "Start RollBack Default User Data");
        
        var result = await _gameDb.DeleteGameDataByUserIdAsync(userId);
        if (result == false)
        {
            LogError(_logger, ErrorCode.FailedRollbackDefaultData, EventType.RollBackDefaultData, "Failed RollBack Default User Data", new { userId });
            return;
        }
        
        LogInfo(_logger, EventType.RollBackDefaultData, "Success RollBack Default User Data", new { userId });
    }
}