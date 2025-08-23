
namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    private static string CreateSessionKey(string email)
    {
        return $"SESSION_{email}";
    }
    
    private static string CreateSessionLockKey(long userId)
    {
        return $"SESSION_LOCK_{userId}";
    }
    
    private static string CreateGameDataKey(long userId)
    {
        return $"GAME_DATA_{userId}";
    }

    private static string CreateStageInfoKey(long userId)
    {
        return $"STAGE_INFO_{userId}";
    }

    private static string CreateQuestKey(long userId)
    {
        return $"QUEST_{userId}";       
    }
}