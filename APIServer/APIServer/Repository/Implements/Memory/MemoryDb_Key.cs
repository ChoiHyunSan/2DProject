
namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    private static string CreateSessionKey(string email)
    {
        return $"SESSION_{email}";
    }
}