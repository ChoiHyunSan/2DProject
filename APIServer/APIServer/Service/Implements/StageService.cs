using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class StageService(ILogger<StageService> logger,IGameDb gameDb, IMemoryDb memoryDb, IMasterDb masterDb, IQuestService questService)
    : IStageService
{
    private readonly ILogger<StageService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;  
    private readonly IMasterDb _masterDb = masterDb;
    private readonly IQuestService _questService = questService;
    
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
            LogError(_logger, ErrorCode.FailedGetClearStage, EventType.GetClearStage, "Failed Get Clear Stage", new { userId, ex.Message, ex.StackTrace });
            return Result<List<StageInfo>>.Failure(ErrorCode.FailedGetClearStage);
        }
    }

    public async Task<Result<List<MonsterInfo>>> EnterStage(long userId, string email, long stageCode, List<long> characterIds)
    {
        try
        {
            // 스테이지 정보 조회
            var monsterInfos = _masterDb.GetStageMonsterList()[stageCode];
        
            // 인게임 정보 캐싱
            var inStageInfo = InStageInfo.Create(userId, email, stageCode, monsterInfos);
            if (await _memoryDb.CacheStageInfo(inStageInfo) == false)
            {
                return Result<List<MonsterInfo>>.Failure(ErrorCode.FailedCacheStageInfo);   
            }
        
            LogInfo(_logger, EventType.EnterStage, "Enter Stage", new { email, stageCode, characterIds });
            
            // 스테이지 정보 반환
            return Result<List<MonsterInfo>>.Success(monsterInfos.Select(x => new MonsterInfo
            {
                monsterCode = x.monster_code,
                monsterCount = x.monster_count
            }).ToList());
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedEnterStage, EventType.EnterStage, "Failed Enter Stage" , new { email, stageCode, ex.Message, ex.StackTrace });
            return Result<List<MonsterInfo>>.Failure(ErrorCode.FailedEnterStage);
        }
    }

    public async Task<Result> ClearStage(long userId, long stageCode, bool clearFlag)
    {
        try
        {
            // 인게임 정보 조회
            if (await _memoryDb.GetGameInfo(userId) is var stageResult && stageResult.IsFailed)
            {
                return stageResult.ErrorCode;
            }
            var stageInfo = stageResult.Value;

            // 클리어 요청 시에 처리 진행. 클리어 실패 시 인게임 정보만 삭제
            if (clearFlag)
            {
                // 클리어 여부 확인 (모든 몬스터를 처치했는지 확인)
                if (CheckStageClear(stageInfo) == false)
                {
                    return ErrorCode.StageInProgress;
                }
        
                // 트랜잭션 처리
                var txResult = await _gameDb.WithTransactionAsync(async q =>
                {
                    // 클리어 확인된 경우, 클리어 정보 추가
                    if (await UpdateClearStageAsync(stageInfo) == false)
                    {
                        return ErrorCode.FailedUpdateClearStage;
                    }
            
                    // 클리어 보상 조회 및 보상 전달
                    if (await RewardClearStageAsync(stageInfo) == false)
                    {
                        return ErrorCode.FailedRewardClearStage;
                    }
                
                    return ErrorCode.None;
                });

                if (txResult != ErrorCode.None)
                {
                    return Result.Failure(txResult);
                }    
            }
            
            // 메모리에 올려진 인게임 정보 삭제
            if(await _memoryDb.DeleteStageInfo(stageInfo) == false)
            {
                return Result.Failure(ErrorCode.FailedDeleteStageInfo);
            }
        
            LogInfo(_logger, EventType.ClearStage, "Clear Stage", new { userId, stageCode });
        
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedClearStage, EventType.ClearStage, "Failed Clear Stage", new { userId, stageCode, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedClearStage);
        }
    }

    public async Task<Result> KillMonster(long userId, long monsterCode)
    {
        try
        {
            // 인게임 정보 조회
            if (await _memoryDb.GetGameInfo(userId) is var stageResult && stageResult.IsFailed)
            {
                return stageResult.ErrorCode;
            }
            var stageInfo = stageResult.Value;
        
            // 몬스터 처치 가능 여부 확인 (몬스터가 존재하는지 & 이미 최대 치로 잡았는지)
            if (VerifyKillMonster(stageInfo, monsterCode) is var verifyResult && verifyResult.IsFailed)
            {
                return verifyResult.ErrorCode;
            }
        
            // 처치 가능한 경우, 처치 숫자 갱신
            if (UpdateKillMonster(stageInfo, monsterCode)  == false)
            {
                return ErrorCode.CannotFindMonsterCode;
            }
        
            // 인게임 정보 갱신
            if (await _memoryDb.CacheStageInfo(stageInfo) == false)
            {
                return Result.Failure(ErrorCode.FailedCacheStageInfo);   
            }
            
            LogInfo(_logger,  EventType.KillMonster, "Kill Monster", new { email = userId, monsterCode });
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedKillMonster, EventType.KillMonster, "Failed Kill Monster", new { email = userId, monsterCode, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedKillMonster);
        }
    }

    private async Task<bool> UpdateClearStageAsync(InStageInfo stageInfo)
    {
        var clearStage = await _gameDb.FindClearStageAsync(stageInfo.userId, stageInfo.stageCode);
        if (await _gameDb.UpdateStageAsync(clearStage) == false)
        {
            return false;
        }
        
        // 퀘스트 갱신 (몬스터 처치 & 스테이지 클리어)
        var totalMonsterKills = stageInfo.monsterKills.Values.Sum();
        var killMonsterQuest = await _questService.RefreshQuestProgress(stageInfo.userId, QuestType.KillMonster, totalMonsterKills);
        var clearStageQuest = await _questService.RefreshQuestProgress(stageInfo.userId, QuestType.ClearStage, (int)stageInfo.stageCode);
        if (killMonsterQuest.IsFailed || clearStageQuest.IsFailed)
        {
            return false;
        }
        
        return true;
    }

    private async Task<bool> RewardClearStageAsync(InStageInfo stageInfo)
    {
        // 보상 정보 & 유저 정보 조회
        var (rewardGold, rewardRune, rewardItem) = GetStageRewards(stageInfo.stageCode);
        var userData = await _gameDb.GetUserDataByEmailAsync(stageInfo.email);
        
        // 골드 재화 획득 & 관련 퀘스트 갱신
        if (await GetGoldReward(userData, rewardGold) == false)
        {
            return false;
        }
        
        // 룬 획득
        if (await GetRuneReward(userData.user_id, rewardRune) == false)
        {
            return false;
        }
        
        // 아이템 획득
        if (await GetItemReward(userData.user_id, rewardItem) == false)
        {
            return false;
        }
        
        // 퀘스트 갱신 (골드 획득 & 아이템 획득)
        var goldQuest = await _questService.RefreshQuestProgress(userData.user_id, QuestType.GetGold, rewardGold.gold);
        var itemQuest = await _questService.RefreshQuestProgress(userData.user_id, QuestType.GetItem, rewardItem.Count);
        if (goldQuest.IsFailed || itemQuest.IsFailed)
        {
            return false;
        }
        
        // 캐시 삭제
        if (await _memoryDb.DeleteCacheData(userData.user_id, [CacheType.Item, CacheType.Rune, CacheType.UserGameData]) is var deleteCache && deleteCache.IsFailed)
        {
            return false;
        }
        
        return true;
    }

    private async Task<bool> GetItemReward(long userId, List<StageRewardItem> rewardItems)
    {
        var dropItems = new List<StageRewardItem>();
        foreach (var item in rewardItems)
        {
            if (item.drop_rate >= Random.Shared.Next(1, 101))
            {
                dropItems.Add(item);
            }
        }

        return await _gameDb.InsertDropItems(userId, dropItems);
    }

    private async Task<bool> GetRuneReward(long userId, List<StageRewardRune> rewardRunes)
    {
        var dropRunes = new List<StageRewardRune>();
        foreach (var item in rewardRunes)
        {
            if (item.drop_rate >= Random.Shared.Next(1, 101))
            {
                dropRunes.Add(item);
            }
        }

        return await _gameDb.InsertDropRunes(userId, dropRunes);
    }

    private async Task<bool> GetGoldReward(UserGameData userData, StageRewardGold rewardGold)
    {
        var newGold = userData.gold + rewardGold.gold;
        return await _gameDb.UpdateUserGoldAsync(userData.user_id, newGold);
    }


    private (StageRewardGold, List<StageRewardRune>, List<StageRewardItem>) GetStageRewards(long stageCode)
    {
        return (_masterDb.GetStageRewardsGold()[stageCode], _masterDb.GetStageRewardsRune()[stageCode], _masterDb.GetStageRewardsItem()[stageCode]);
    }

    private bool CheckStageClear(InStageInfo s)
    {
        return s.monsterKillTargets.Count == s.monsterKills.Count
               && !s.monsterKillTargets.Except(s.monsterKills).Any();
    }

    private bool UpdateKillMonster(InStageInfo stageInfo, long monsterCode)
    {
        if (stageInfo.monsterKills.TryGetValue(monsterCode, out var killCount))
        {
            stageInfo.monsterKills[monsterCode] = killCount + 1;
            return true;
        }

        return false;
    }

    private Result VerifyKillMonster(InStageInfo stageInfo, long monsterCode)
    {
        var targetCounts = stageInfo.monsterKillTargets.GetValueOrDefault(monsterCode, 0);
        var killCounts = stageInfo.monsterKills.GetValueOrDefault(monsterCode, 0);
        if (targetCounts == 0 )
        {
            return ErrorCode.CannotFindMonsterCode;
        }

        if (targetCounts <= killCounts)
        {
            return ErrorCode.CannotKillMonster;
        }

        return ErrorCode.None;
    }
}