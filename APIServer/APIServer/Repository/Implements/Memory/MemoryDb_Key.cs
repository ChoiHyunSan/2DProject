
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
    
    private static string CreateUserGameDataKey(long userId)
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
    
    private static string CreateCharacterDataKey(long userId)
    {
        return $"CHARACTER_DATA_{userId}";
    }
    
    private static string CreateItemDataKey(long userId)
    {
        return $"ITEM_DATA_{userId}";
    }
    
    private static string CreateRuneDataKey(long userId)
    {
        return $"RUNE_DATA_{userId}";
    }
}