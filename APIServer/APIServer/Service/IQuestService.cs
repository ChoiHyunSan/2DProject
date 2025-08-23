using APIServer.Models.Entity.Data;

namespace APIServer.Service;

public interface IQuestService
{
    Task<Result> RewardQuest(long userId, long questCode);

    Task<Result> RefreshQuestProgress(long userId, QuestType type, int addValue);
}