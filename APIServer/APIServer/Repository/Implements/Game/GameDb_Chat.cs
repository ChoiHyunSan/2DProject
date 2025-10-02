using APIServer.Models.Entity;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<bool> InsertChatLogAsync(ChatLog chatLog)
    {
        var ok = await _queryFactory.Query(TABLE_CHAT_LOG).InsertAsync(new
        {
            redis_stream_id = chatLog.redis_stream_id,
            email           = chatLog.email,
            message         = chatLog.message,
            send_at_utc     = chatLog.send_at_utc
        });

        return ok == 1;
    }
}