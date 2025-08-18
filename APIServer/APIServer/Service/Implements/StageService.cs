using APIServer.Models.DTO;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using APIServer.Repository;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class StageService(ILogger<StageService> logger,IGameDb gameDb, IMemoryDb memoryDb, IMasterDb masterDb)
    : IStageService
{
    private readonly ILogger<StageService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;  
    private readonly IMasterDb _masterDb = masterDb;
    
    public async Task<Result<List<StageInfo>>> GetClearStage(long userId)
    {
        LogInfo(_logger, EventType.GetClearStage, "Get Clear Stage", new { userId });
        
        var stagesResult = await _gameDb.GetClearStageList(userId);
        if(stagesResult.IsFailed) return Result<List<StageInfo>>.Failure(stagesResult.ErrorCode);
        
        var stageInfos = stagesResult.Value
            .Select(stage => new StageInfo
            {
                stageCode = stage.stageCode,
                clearCount = stage.clearCount,
                lastClearDate = stage.lastClearDate
            }).ToList();
        
        return Result<List<StageInfo>>.Success(stageInfos);
    }

    public async Task<Result<List<MonsterInfo>>> EnterStage(long userId, string email, long stageCode, List<long> characterIds)
    {
        LogInfo(_logger, EventType.EnterStage, "Enter Stage", new { email, stageCode, characterIds });
        
        // 스테이지 정보 조회
        var result = await _masterDb.GetStageMonsterListAsync(stageCode);
        if(result.IsFailed) return Result<List<MonsterInfo>>.Failure(result.ErrorCode);

        var monsterInfos = result.Value;
        
        // 인게임 정보 캐싱
        var inStageInfo = InStageInfo.Create(userId, email, stageCode, monsterInfos);
        var cacheResult = await _memoryDb.CacheStageInfo(inStageInfo);
        if(cacheResult.IsFailed) return  Result<List<MonsterInfo>>.Failure(cacheResult.ErrorCode);
        
        // 스테이지 정보 반환
        return Result<List<MonsterInfo>>.Success(monsterInfos.Select(x => new MonsterInfo
        {
            monsterCode = x.monsterCode,
            monsterCount = x.monsterCount
        }).ToList());
    }

    public async Task<Result> ClearStage(string email, long stageCode)
    {
        LogInfo(_logger, EventType.ClearStage, "Clear Stage", new { email, stageCode });
        
        // 인게임 정보 조회
        var result = await memoryDb.GetGameInfo(email);
        if(result.IsFailed) return Result.Failure(result.ErrorCode);

        var stageInfo = result.Value;
        
        // 클리어 여부 확인 (모든 몬스터를 처치했는지 확인)
        var clearCheck = await CheckStageClear(stageInfo);
        if(clearCheck.IsFailed) return Result.Failure(clearCheck.ErrorCode);

        // 보상 정보와 유저 정보 조회
        var goldReward = await _masterDb.GetGoldReward(stageInfo.stageCode);
        var gemReward = await _masterDb.GetGemReward(stageInfo.stageCode);
        var expReward = await _masterDb.GetExpReward(stageInfo.stageCode);
        if (goldReward.IsFailed || gemReward.IsFailed || expReward.IsFailed)
        {
            return Result.Failure(goldReward.ErrorCode);
        }
        
        var userData = await _gameDb.GetUserDataByEmailAsync(email);
        
        
        var txResult = await _gameDb.WithTransactionAsync<Result>(async q =>
        {
            // 클리어 확인된 경우, 클리어 정보 추가
            var clearResult = await _gameDb.UpdateClearStageAsync(stageInfo.userId, stageInfo.stageCode);
            if(clearResult.IsFailed) return Result.Failure(clearResult.ErrorCode);
            
            // 클리어 보상 조회 및 보상 전달
            var rewardResult = await _gameDb.RewardClearStage(stageInfo);
            if(rewardResult.IsFailed) return Result.Failure(rewardResult.ErrorCode);

            return Result.Success();
        });
        
        // 메모리에 올려진 인게임 정보 삭제
        var deleteResult = await _memoryDb.DeleteStageInfo(stageInfo);
        if(deleteResult.IsFailed) return  Result.Failure(deleteResult.ErrorCode);
        
        return Result.Success();
    }
    
    public async Task<Result> KillMonster(string email, long monsterCode)
    {
        LogInfo(_logger,  EventType.KillMonster, "Kill Monster", new { email, monsterCode });
        
        // 인게임 정보 조회
        var inGameResult = await _memoryDb.GetGameInfo(email);
        if(inGameResult.IsFailed) return Result.Failure(inGameResult.ErrorCode);
        
        // 몬스터 처치 가능 여부 확인 (몬스터가 존재하는지 & 이미 최대 치로 잡았는지)
        var stageInfo = inGameResult.Value;
        var verifyKillResult = await VerifyKillMonster(stageInfo, monsterCode);
        if (verifyKillResult.IsFailed) return Result.Failure(verifyKillResult.ErrorCode);
        
        // 처치 가능한 경우, 처치 숫자 갱신
        var updateKillResult = await UpdateKillMonster(stageInfo, monsterCode);
        if(updateKillResult.IsFailed) return Result.Failure(updateKillResult.ErrorCode);
        
        // 인게임 정보 갱신
        var updateStageInfo = await _memoryDb.CacheStageInfo(stageInfo);
        if(updateStageInfo.IsFailed) return Result.Failure(updateStageInfo.ErrorCode);
        
        return Result.Success();
    }

    private async Task<Result> CheckStageClear(InStageInfo stageInfo)
    {
        var monsterKillTargets = stageInfo.monsterKillTargets;
        var monsterKills = stageInfo.monsterKills;

        foreach (var monsterKillTarget in monsterKillTargets)
        {
            var (code, count) = (monsterKillTarget.Key, monsterKillTarget.Value);
            
            var killCount = monsterKills[code];
            if (count != killCount)
            {
                LogError(_logger, ErrorCode.StageInProgress, EventType.ClearStage, 
                    "Stage is InProgress", new { stageInfo.email, stageInfo.stageCode });
                return ErrorCode.StageInProgress;
            }
        }
        
        return Result.Success();
    }
    
    private async Task<Result> UpdateKillMonster(InStageInfo stageInfo, long monsterCode)
    {
        var killCounts = stageInfo.monsterKills.GetValueOrDefault(monsterCode, -1);
        if (killCounts == -1)
        {
            LogError(_logger, ErrorCode.CannotFindMonsterCode, EventType.KillMonster
                , "Cannot Find Monster Code In StageInfo", new { stageInfo.stageCode, monsterCode });
            return ErrorCode.CannotFindMonsterCode;
        }

        killCounts += 1;
        stageInfo.monsterKills[monsterCode] = killCounts;
        
        return Result.Success();
    }

    private async Task<Result> VerifyKillMonster(InStageInfo stageInfo, long monsterCode)
    {
        var targetCounts = stageInfo.monsterKillTargets.GetValueOrDefault(monsterCode, 0);
        var killCounts = stageInfo.monsterKills.GetValueOrDefault(monsterCode, 0);
        if (targetCounts == 0 )
        {
            LogError(_logger, ErrorCode.CannotFindMonsterCode, EventType.KillMonster
                , "Cannot Find Monster Code In StageInfo", new { stageInfo.stageCode, monsterCode });
            return ErrorCode.CannotFindMonsterCode;
        }

        if (targetCounts <=killCounts)
        {
            LogError(_logger, ErrorCode.CannotKillMonster, EventType.KillMonster,
                "Already Kill All This Type Monsters", new { stageInfo.stageCode, monsterCode , targetCounts });
            return ErrorCode.CannotKillMonster;
        }

        return Result.Success();
    }
}