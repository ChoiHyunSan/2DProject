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
        try
        {
            // 클리어 스테이지 조회
            var stageList = await _gameDb.GetClearStageListAsync(userId);
            
            // 데이터 양식 변경
            var stageInfos = stageList.Select(StageInfo.Of).ToList();

            LogInfo(_logger, EventType.GetClearStage, "Get Clear Stage", new { userId });

            return Result<List<StageInfo>>.Success(stageInfos);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedGetClearStage, EventType.GetClearStage
            , "Failed Get Clear Stage", new { userId, ex.Message, ex.StackTrace });

            return Result<List<StageInfo>>.Failure(ErrorCode.FailedGetClearStage);
        }
    }

    public async Task<Result<List<MonsterInfo>>> EnterStage(long userId, string email, long stageCode, List<long> characterIds)
    {
        LogInfo(_logger, EventType.EnterStage, "Enter Stage", new { email, stageCode, characterIds });
        
        // 스테이지 정보 조회
        var monsterInfos = _masterDb.GetStageMonsterList()[stageCode];
        
        // 인게임 정보 캐싱
        var inStageInfo = InStageInfo.Create(userId, email, stageCode, monsterInfos);
        if (await _memoryDb.CacheStageInfo(inStageInfo) == false)
        {
            return Result<List<MonsterInfo>>.Failure(ErrorCode.FailedCacheStageInfo);   
        }
        
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
        var stageResult = await _memoryDb.GetGameInfo(email);
        if (stageResult.IsFailed) return stageResult.ErrorCode;
        var stageInfo = stageResult.Value;
        
        // 클리어 여부 확인 (모든 몬스터를 처치했는지 확인)
        if (await CheckStageClearAsync(stageInfo) == false)
        {
            return ErrorCode.StageInProgress;
        }
        
        // 보상 정보와 유저 정보 조회
        var rewards = GetStageRewards(stageInfo.stageCode);

        var userData = await _gameDb.GetUserDataByEmailAsync(email);
        var txResult = await _gameDb.WithTransactionAsync<Result>(async q =>
        {
            // 클리어 확인된 경우, 클리어 정보 추가
            if (await _gameDb.UpdateClearStageAsync(stageInfo.userId, stageInfo.stageCode) == false)
            {
                return ErrorCode.FailedUpdateClearStage;
            }
            
            // 클리어 보상 조회 및 보상 전달
            if (await _gameDb.RewardClearStageAsync(stageInfo) == false)
            {
                return ErrorCode.FailedRewardClearStage;
            }

            return Result.Success();
        });
        
        // 메모리에 올려진 인게임 정보 삭제
        if(await _memoryDb.DeleteStageInfo(stageInfo) == false)
        {
            return Result.Failure(ErrorCode.FailedDeleteStageInfo);
        }
        
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
        if (await VerifyKillMonster(stageInfo, monsterCode) is { IsFailed: true } result)
        {
            return result.ErrorCode;
        }
        
        // 처치 가능한 경우, 처치 숫자 갱신
        var updateKillResult = await UpdateKillMonster(stageInfo, monsterCode);
        if(updateKillResult.IsFailed) return Result.Failure(updateKillResult.ErrorCode);
        
        // 인게임 정보 갱신
        if (await _memoryDb.CacheStageInfo(stageInfo) == false)
        {
           return Result.Failure(ErrorCode.FailedCacheStageInfo);   
        }
        
        return Result.Success();
    }

    private (StageRewardGold, StageRewardRune, StageRewardItem) GetStageRewards(long stageCode)
    {
        return (masterDb.GetStageRewardsGold()[stageCode], _masterDb.GetStageRewardsRune()[stageCode], _masterDb.GetStageRewardsItem()[stageCode]);
    }

    private async Task<bool> CheckStageClearAsync(InStageInfo stageInfo)
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
                return false;
            }
        }

        return true;
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