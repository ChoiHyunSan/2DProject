using System.Text.Json;
using APIServer.Models.DTO.Chat;
using StackExchange.Redis;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    private const string ChatKey = "chat:global";
    private const int ChatMaxLen = 10_000; 

    public async Task<ChatMessage?> SendChatAsync(string email, string message)
    {
        try
        {
            var db = _conn.GetConnection().GetDatabase();
            
            var nowUtc = DateTime.UtcNow;
            var payload = new { email, sendAt = nowUtc, message };
            var json = JsonSerializer.Serialize(payload);
            
            var id = await db.StreamAddAsync(
                key: ChatKey,
                streamField: "",
                streamValue: json,
                maxLength: ChatMaxLen,
                useApproximateMaxLength: true
            );

            if (!id.HasValue)
                return null;

            var idStr = id.ToString(); 

            return new ChatMessage
            {
                messageId = idStr,
                email = email,
                sendAt = nowUtc,
                message = message
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ChatMessage>> FetchChatsAsync(int count, string? afterMessageId)
    {
        try
        {
            var db = _conn.GetConnection().GetDatabase();
            StreamEntry[] loaded;

            if (string.IsNullOrEmpty(afterMessageId))
            {
                // 최신 순인 내림 차순으로 읽기
                loaded = await db.StreamRangeAsync(
                    key: ChatKey,
                    minId: "-",
                    maxId: "+",
                    count: count,
                    messageOrder: Order.Descending 
                );
            }
            else
            {
                loaded = await db.StreamReadAsync(
                    key: ChatKey,
                    position: afterMessageId,
                    count: count
                );
            }

            if (loaded is null || loaded.Length == 0)
                return [];

            var list = DeserializeChatMessages(loaded);

            // 리스트에서 가장 오래된 순으로 오름차순 정렬로 수정
            if (string.IsNullOrEmpty(afterMessageId))
                list.Reverse();

            return list;
        }
        catch
        {
            return [];
        }
    }

    private List<ChatMessage> DeserializeChatMessages(StreamEntry[] streams)
    {
        var result = new List<ChatMessage>(streams.Length);

        foreach (var e in streams)
        {
            if (e.Values == null || e.Values.Length == 0)
                continue;

            // 단일 필드
            var raw = e.Values[0].Value.ToString();
            if (string.IsNullOrEmpty(raw))
                continue;

            var msg = JsonSerializer.Deserialize<ChatMessage>(raw);
            if (msg == null) continue;
            msg.messageId = e.Id;
            
            result.Add(msg);
        }

        return result;
    }
}
