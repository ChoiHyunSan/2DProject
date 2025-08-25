using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Repository;
using StackExchange.Redis;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class QuestService(ILogger<QuestService> logger, IGameDb gameDb, IDataLoadService dataLoadService, IMasterDb masterDb, IMemoryDb memoryDb)
    : IQuestService
{
    private readonly ILogger<QuestService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMasterDb _masterDb = masterDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    public async Task<Result> RewardQuest(long userId, long questCode)
    {
        try
        {
            // 퀘스트 조회
            var completeQuest = await _gameDb.GetCompleteQuest(userId, questCode);

            // 이미 보상을 제공했는지 확인
            if (completeQuest.earn_reward)
            {
                return Result.Failure(ErrorCode.AlreadyEarnReward);
            }

            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 보상 제공
                if (await EarnQuestReward(userId, questCode) == false)
                {
                    return ErrorCode.FailedRewardQuest;
                }
                
                // 보상 제공 확인 처리
                if (await _gameDb.RewardCompleteQuest(userId, questCode) == false)
                {
                    return ErrorCode.FailedRewardQuest;
                }

                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return Result.Failure(txErrorCode);
            }
            
            LogInfo(_logger, EventType.RewardQuest, "Reward Quest", new { userId, questCode });
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedRewardQuest, EventType.RewardQuest,
                "Failed Reward Quest", new { userId, questCode, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedRewardQuest);
        }
    }

    public async Task<Result> RefreshQuestProgress(long userId, QuestType type, int addValue)
    {
        try
        {
            // 1) DB에서 진행중 퀘스트 조회
            var quests = await _gameDb.GetProgressQuestByType(userId, type);

            // 2) 한 번의 순회로 완료/미완료 분리 + 갱신 진행도 저장
            var completed = new HashSet<long>();
            var updatedProgress = new Dictionary<long, int>(quests.Count);

            var questInfoMap = _masterDb.GetQuestInfoDatas(); 

            foreach (var q in quests)
            {
                var target = questInfoMap[q.quest_code];
                if (target.quest_type == QuestType.ClearStage)
                {
                    if(addValue == target.quest_progress)
                        completed.Add(q.quest_code);
                }
                else
                {
                    var newProgress = q.progress + addValue;
                
                    if (newProgress >= target.quest_progress)
                        completed.Add(q.quest_code);
                    else
                        updatedProgress[q.quest_code] = newProgress;    
                }
            }

            // 3) 완료 처리 (DB)
            if (completed.Count > 0)
            {
                var ok = await _gameDb.CompleteQuest(userId, completed.ToList());
                if (!ok)
                    return ErrorCode.FailedCompleteQuest;
            }

            // 4) 캐시가 있으면 메모리 갱신
            var cache = await _memoryDb.GetCachedQuestList(userId);
            if (cache.IsFailed)
            {
                // 캐시 없으면 여기서 끝
                return Result.Success();
            }

            var cached = cache.Value; 

            // 4-1) 완료된 퀘스트는 캐시에서 제거 
            if (completed.Count > 0)
            {
                cached.RemoveAll(q => completed.Contains(q.quest_code));
            }

            // 4-2) 미완료 퀘스트의 진행도 반영 
            foreach (var q in cached)
            {
                if (updatedProgress.TryGetValue(q.quest_code, out var newProg))
                {
                    // 안전하게 clamp (선택)
                    var target = questInfoMap[q.quest_code].quest_progress;
                    q.progress = newProg;
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedRefreshQuest, EventType.RefreshQuest,
                "Faile Refresh Quest Progress ", new { userId, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedRefreshQuest);    
        }
    }

    private async Task<bool> EarnQuestReward(long userId, long questCode)
    {
        // 퀘스트 정보 조회
        var quest = _masterDb.GetQuestInfoDatas()[questCode];
        var (gold, gem, exp) = (quest.reward_gold, quest.reward_gem, quest.reward_exp);
        
        // 유저 데이터 조회
        var userData = await _gameDb.GetUserDataByUserIdAsync(userId);
        var (curGold, curGem, curExp, curLevel) = (userData.gold, userData.gem, userData.exp, userData.level);
        
        // 갱신할 데이터 계산
        var (newGold, newGem) = (curGold + gold, curGem + gem);
        var (newLevel, newExp) = (curLevel, curExp + exp);
        
        // 경험치는 레벨 공통으로 100당 1레벨로 산정해서 계산
        if (newExp >= 100)
        {
            newLevel += newExp / 100;
            curExp = newExp % 100;
        }
        
        // 정보 갱신
        if (await _gameDb.UpdateUserCurrencyAsync(userId, newGold, newGem, newLevel, newExp) == false)
        {
            return false;
        }

        return true;
    }
}