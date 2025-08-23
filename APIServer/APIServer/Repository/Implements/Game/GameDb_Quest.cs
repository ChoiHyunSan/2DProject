using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<List<UserQuestInprogress>> GetProgressQuestList(long userId)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_INPROGRESS)
            .Where(USER_ID, userId)
            .GetAsync<UserQuestInprogress>();

        return result.ToList();
    }

    public async Task<List<UserQuestComplete>> GetCompleteQuestList(long userId, Pageable pageable)
    {
        var offset = (pageable.page - 1) * pageable.size;
        
        var result = await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .Limit(pageable.size)
            .Offset(offset)
            .GetAsync<UserQuestComplete>();

        return result.ToList();
    }

    public async Task<UserQuestComplete> GetCompleteQuest(long userId, long questCode)
    {
        return await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .Where(QUEST_CODE, questCode)
            .FirstOrDefaultAsync<UserQuestComplete>();
    }

    public async Task<bool> RewardCompleteQuest(long userId, long questCode)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                EARN_REWARD = true
            });

        return result == 1;
    }

    public async Task<List<UserQuestInprogress>> GetProgressQuestByType(long userId, QuestType type)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_INPROGRESS)
            .Where(USER_ID, userId)
            .Where(QUEST_TYPE, type)
            .GetAsync<UserQuestInprogress>();

        return result.ToList();
    }

    public Task<bool> CompleteQuest(long userId, List<long> completeQuest)
    {
        throw new NotImplementedException();
    }
}