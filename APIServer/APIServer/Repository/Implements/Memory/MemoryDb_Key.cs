
namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    private static string CreateSessionKey(string email)
    {
        return $"SESSION_{email}";
    }
    
    private static string CreateSessionLockKey(string email)
    {
        return $"SESSION_LOCK_{email}";
    }
    
    private static string CreateGameDataKey(string email)
    {
        return $"GAME_DATA_{email}";
    }
}