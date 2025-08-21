namespace APIServer.Service;

public interface IQuestService
{
    Task<Result> RewardQuest(long userId, long questCode);
}