using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    /// <summary>
    /// 캐릭터 구매 요청 메서드
    /// 1) 현재 UserData의 Gold, Gem 조회
    /// 2) 구매할 수 있는지 비교
    /// 3) 구매 가능한 경우, 재화를 차감하여 캐릭터 구매
    /// 4) 계산 결과 DB에 반영
    ///
    /// - 3 ~ 4번은 하나의 트랜잭션으로 묶인 상태로 작업한다. 
    /// 반환 값 : (구매 결과 에러 코드, 현재 남은 골드 재화, 현재 남은 유료 재화)
    /// </summary>
    public async Task<(ErrorCode, int , int)> PurchaseCharacter(long userId, long characterCode, int goldPrice, int gemPrice)
    {
        var (checkErrorCode, isAlreadyHave) = await CheckAlreadyHaveCharacter(userId, characterCode);
        if (checkErrorCode != ErrorCode.None || isAlreadyHave)
        {
            return (checkErrorCode, 0, 0);
        }
        
        var (errorCode, currentGold, currentGem) = await GetGoldAndGem(userId);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, currentGold, currentGem);
        }

        if (goldPrice > currentGold || gemPrice > currentGem)
        {
            return (ErrorCode.CannotPurchaseCharacter, currentGold, currentGem);
        }

        var newGold = currentGold - goldPrice;
        var newGem = currentGem - gemPrice;
        
        var txCode = await WithTransactionAsync(async q =>
        {
            var e1 = await UpdateGoldAndGem(userId, newGold, newGem);
            if (e1 != ErrorCode.None)
                return e1;

            var e2 = await InsertNewCharacter(userId, characterCode);
            if (e2 != ErrorCode.None)
                return e2;

            return ErrorCode.None;
        });

        if (txCode != ErrorCode.None)
        {
            return (txCode, currentGold, currentGem);    
        }

        return (ErrorCode.None, newGold, newGem);
    }

    private async Task<(ErrorCode, bool)> CheckAlreadyHaveCharacter(long userId, long characterCode)
    {
        try
        {
            LogInfo(_logger, EventType.CheckAlreadyHaveCharacter, "Check Already Have Character", new { userId, characterCode });

            var cnt = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .Where(USER_ID, userId)
                .Where(CHARACTER_CODE, characterCode)
                .CountAsync<long>();
            
            return cnt > 0 
                ? (ErrorCode.AlreadyHaveCharacter, true) 
                : (ErrorCode.None, false);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadUserData, EventType.CheckAlreadyHaveCharacter, "Failed Check Already Have Character", new
            {
                userId, characterCode,
                e.Message,
                e.StackTrace
            });
            return (ErrorCode.FailedLoadUserData, false);
        }
    }
    
    private async Task<(ErrorCode, int, int)> GetGoldAndGem(long userId)
    {
        try
        {
            LogInfo(_logger, EventType.GetGoldAndGem, "Get Gold And Gem", new { userId });
            
            var (gold, gem) = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .Select(GOLD, GEM)
                .FirstOrDefaultAsync<(int, int)>();
            
            return (ErrorCode.None, gold, gem);
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedLoadAllGameData , EventType.GetGoldAndGem, "Failed Get Gold And Gem", new
            {
                userId,
                e.Message,
                e.StackTrace
            });
            return (ErrorCode.FailedLoadAllGameData, 0, 0);
        }
    }

    private async Task<ErrorCode> UpdateGoldAndGem(long userId, int newGold, int newGem)
    {
        try
        {
            LogInfo(_logger, EventType.UpdateGoldAndGem, "Update Gold And Gem", new { userId, newGold, newGem });

            var result = await _queryFactory.Query(TABLE_USER_GAME_DATA)
                .Where(USER_ID, userId)
                .UpdateAsync(new
                {
                    GOLD = newGold,
                    GEM = newGem,
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateGoldAndGem, "Failed Update Gold And Gem", new
                {
                    userId
                });
                return ErrorCode.FailedUpdateGoldAndGem;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedUpdateGoldAndGem, EventType.UpdateGoldAndGem, "Failed Update Gold And Gem", new
            {
                userId, 
                e.Message,
                e.StackTrace
            });
            return ErrorCode.FailedUpdateGoldAndGem;
        }
    }

    private async Task<ErrorCode> InsertNewCharacter(long userId, long characterCode)
    {
        try
        {
            LogInfo(_logger, EventType.InsertNewCharacter, "Insert New Character", new { userId, characterCode });

            var result = await _queryFactory.Query(TABLE_USER_INVENTORY_CHARACTER)
                .InsertAsync(new
                {
                    USER_ID = userId,
                    LEVEL = 1,
                    CHARACTER_CODE = characterCode
                });

            if (result == 0)
            {
                LogError(_logger, ErrorCode.CannotInsertNewCharacter ,EventType.InsertNewCharacter, "Failed Insert New Character", new
                {
                    userId, 
                    characterCode
                });
                return ErrorCode.CannotInsertNewCharacter;
            }
            
            return ErrorCode.None;
        }
        catch (Exception e)
        {
           LogError(_logger, ErrorCode.FailedInsertNewCharacter, EventType.InsertNewCharacter, "Failed Insert New Character", new
           {
               userId, characterCode,
               e.Message,
               e.StackTrace
           });
           return ErrorCode.FailedInsertNewCharacter;
        }
    }
}