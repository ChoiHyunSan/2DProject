using APIServer.Repository;

namespace APIServer.Service.Implements;

public class QuestService(ILogger<QuestService> logger, IGameDb gameDb, IDataLoadService dataLoadService)
    : IQuestService
{
    private readonly ILogger<QuestService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    
    public async Task<Result> RewardQuest(long userId, long questCode)
    {
        try
        {
            // 퀘스트 조회
            var completeQuest = await _gameDb.GetCompleteQuest(userId, questCode);

            // 이미 보상을 제공했는지 확인
            if (completeQuest.earnReward)
            {
                return Result.Failure(ErrorCode.AlreadyEarnReward);
            }

            // 보상 제공
            if (await _gameDb.RewardCompleteQuest(userId, questCode) == false)
            {
                return Result.Failure(ErrorCode.FailedRewardQuest);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LoggerManager.LogError(_logger, ErrorCode.FailedRewardQuest, EventType.RewardQuest,
                "Faile Reward Quest", new { userId, questCode, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedRewardQuest);
        }
    }
}