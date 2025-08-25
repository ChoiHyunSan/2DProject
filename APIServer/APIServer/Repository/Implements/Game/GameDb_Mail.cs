using APIServer.Models.DTO;
using APIServer.Models.Entity;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<UserMail> GetMailAsync(long userId, long mailId)
    {
        return await _queryFactory.Query(TABLE_USER_MAIL)
            .Where(USER_ID, userId)
            .Where(MAIL_ID, mailId)
            .FirstOrDefaultAsync<UserMail>();   
            
    }

    public async Task<bool> ReceiveCompleteMailAsync(long mailId)
    {
        var result = await _queryFactory.Query(TABLE_USER_MAIL)
            .Where(MAIL_ID, mailId)
            .UpdateAsync(new
            {
                earn_reward = true
            });

        return result == 1;
    }

    public async Task<bool> InsertNewMail(UserMail newMail)
    {
        var result = await _queryFactory.Query(TABLE_USER_MAIL)
            .InsertAsync(new
            {
                newMail.mail_id,
                newMail.user_id,
                newMail.mail_title,
                newMail.reward_code,
                newMail.count,
                newMail.earn_reward,
                newMail.send_date,
                newMail.expire_date
            });

        return result == 1;
    }

    public async Task<List<UserMail>> GetUnReceiveMailByPaging(long userId, Pageable pageable)
    {
        var offset = (pageable.page - 1) * pageable.size;
        var result = await _queryFactory.Query(TABLE_USER_MAIL)
            .Where(USER_ID, userId)
            .Where(RECEIVE_DATE, null)
            .Offset(offset)
            .Limit(pageable.size)
            .GetAsync<UserMail>();

        return result.ToList();
    }
}