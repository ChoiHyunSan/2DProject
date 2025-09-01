using APIServer.Models.Entity;
using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class AccountService(ILogger<AccountService> logger, IAccountDb accountDb, IGameDb gameDb, IMemoryDb memoryDb, ISecurityService securityService) 
    : IAccountService
{
    private readonly ILogger<AccountService> _logger = logger;
    private readonly IAccountDb _accountDb = accountDb;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    private readonly ISecurityService _securityService = securityService;
    
    public async Task<Result> RegisterAccountAsync(string email, string password)
    {
        try
        {
            // 중복 이메일 조회
            if (await _accountDb.CheckExistAccountByEmailAsync(email))
            {
                return ErrorCode.DuplicatedEmail;
            }

            // 기본 유저 게임데이터 생성
            if (await CreateDefaultUserGameDataAsync() is var created && created.IsFailed)
            {
                return created.ErrorCode;
            }

            var userId = created.Value;
            var saltValue = SecurityUtils.GenerateSalt();
            var (_, hashPassword) = _securityService.HashPassword(password, saltValue);
            
            // 계정 정보 생성
            if (await _accountDb.CreateAccountUserDataAsync(userId, email, saltValue, hashPassword) == false)
            {
                await RollbackCreateDefaultUserGameDataAsync(userId);
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedRegister, EventType.Register, 
                    "Failed Register User Account", new { email, ex.Message, ex.StackTrace });
            return ErrorCode.FailedRegister;
        }
    }
    
    public async Task<Result<(long, string)>> LoginAsync(string email, string password)
    {
        try
        {
            // 계정 조회
            var account = await _accountDb.GetUserAccountByEmailAsync(email);
            if (account is null)
            {
                return Result<(long, string)>.Failure(ErrorCode.CannotFindAccountUser);
            }
            
            // 패스워드 검증
            if (!_securityService.VerifyPassword(account.password, account.salt_value, password))
            {
                return Result<(long, string)>.Failure(ErrorCode.FailedPasswordVerify);   
            }

            // 토큰 발급 + 세션 등록
            var token = _securityService.GenerateAuthToken();
            if (await _memoryDb.RegisterSessionAsync(CreateNewSession(account, token)) == false)
            {
                return Result<(long, string)>.Failure(ErrorCode.FailedRegisterSession);
            }

            return Result<(long, string)>.Success((account.user_id, token));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedLogin, EventType.Login, 
                "Failed User Login", new { email, ex.Message, ex.StackTrace });
            return Result<(long, string)>.Failure(ErrorCode.FailedLogin);
        }
    }

    private static UserSession CreateNewSession(UserAccount account, string authToken)
    {
        return new UserSession
        {
            accountId = account.account_id,
            authToken = authToken,
            createDate = DateTime.Now,
            email = account.email,
            userId = account.user_id
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
        return await _gameDb.InsertAttendanceMonthAsync(userId);
    }

    private async Task<bool> CreateDefaultCharacterAsync(long userId)
    {
        var defaultCharacters = new[]
        {
            new UserInventoryCharacter { character_code = 30001, level = 1 }
        };
        
        foreach (var character in defaultCharacters)
        {
            var result = await _gameDb.InsertNewCharacterAsync(userId, character.character_code);
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
            new UserInventoryRune { rune_code = 20000, level = 1 },
            new UserInventoryRune { rune_code = 20001, level = 1},
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
            new UserInventoryItem { item_code = 10001, level = 1 },
            new UserInventoryItem { item_code = 10002, level = 1},
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