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
        // 1) 중복 이메일 가드
        var exists = await _accountDb.CheckExistAccountByEmailAsync(email);
        if (exists.IsFailed)
            return exists.ErrorCode;

        if (exists.Value) // true면 이미 존재
            return ErrorCode.DuplicatedEmail;

        // 2) 기본 유저 게임데이터 생성
        var created = await CreateDefaultUserGameDataAsync();
        if (created.IsFailed)
            return created.ErrorCode;

        var userId = created.Value;

        // 3) 계정-유저 매핑 생성
        var linked = await _accountDb.CreateAccountUserDataAsync(userId, email, password);
        if (linked.IsFailed)
        {
            // 위 단계 일부 성공 시에만 롤백
            await RollbackCreateDefaultUserGameDataAsync(userId);
            return linked.ErrorCode;
        }

        return ErrorCode.None;
    }
    
    public async Task<Result<(GameData, string)>> LoginAsync(string email, string password)
    {
        // 1) 계정 조회
        var accountRes = await _accountDb.GetUserAccountByEmailAsync(email);
        if (accountRes.IsFailed)
            return Result<(GameData, string)>.Failure(accountRes.ErrorCode);

        var account = accountRes.Value;

        // 2) 패스워드 검증 (가드)
        if (!SecurityUtils.VerifyPassword(account.password, account.saltValue, password))
            return Result<(GameData, string)>.Failure(ErrorCode.FailedPasswordVerify);

        // 3) 게임데이터 조회
        var gameRes = await _gameDb.GetAllGameDataByUserIdAsync(account.userId);
        if (gameRes.IsFailed)
            return Result<(GameData, string)>.Failure(gameRes.ErrorCode);

        // 4) 토큰 발급 + 세션 등록
        var token = SecurityUtils.GenerateAuthToken();
        var sessionCode = await _memoryDb.RegisterSessionAsync(CreateNewSession(account, token));
        if (sessionCode != ErrorCode.None)
            return Result<(GameData, string)>.Failure(ErrorCode.FailedRegisterSession);

        // 5) 캐시 저장
        var cacheRes = await _memoryDb.CacheGameData(email, gameRes.Value);
        if (cacheRes.IsFailed)
            return Result<(GameData, string)>.Failure(ErrorCode.FailedCacheGameData);

        return Result<(GameData, string)>.Success((gameRes.Value, token));
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
        
        var result = await _gameDb.CreateUserGameDataAndReturnUserIdAsync();
        var userId = result.Value;
        
        if (result.IsFailed || userId == 0 ||
            await CreateDefaultCharacterAsync(userId) != ErrorCode.None ||
            await CreateDefaultItemAsync(userId) != ErrorCode.None ||
            await CreateDefaultRuneAsync(userId) != ErrorCode.None ||
            await CreateDefaultAttendanceAsync(userId) != ErrorCode.None ||
            await CreateDefaultQuestAsync(userId) != ErrorCode.None
            )
        {
            LogError(_logger, ErrorCode.FailedInsertData, EventType.CreateDefaultData, "Failed Create Default User Data");
            return Result<long>.Failure(ErrorCode.FailedInsertData, userId);     
        }
        
        LogInfo(_logger, EventType.CreateDefaultData, "Success Create Default User Data", new { userId });
        
        return Result<long>.Success(userId);
    }

    private async Task<Result> CreateDefaultQuestAsync(long userId)
    {
        var result =  await _gameDb.InsertQuestAsync(userId, 60000, DateTime.Now.AddYears(1));
        return result.ErrorCode;
    }

    private async Task<Result> CreateDefaultAttendanceAsync(long userId)
    {
        var result = await _gameDb.InsertAttendanceMonthAsync(userId);
        if (result != ErrorCode.None)
        {
            return result.ErrorCode;      
        }
        
        result = await _gameDb.InsertAttendanceWeekAsync(userId);
        return result.ErrorCode;
    }

    private async Task<Result> CreateDefaultCharacterAsync(long userId)
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
    
    private async Task<Result> CreateDefaultRuneAsync(long userId)
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

    private async Task<Result> CreateDefaultItemAsync(long userId)
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