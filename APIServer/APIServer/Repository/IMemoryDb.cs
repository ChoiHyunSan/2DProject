using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IMemoryDb
{
    Task<bool> RegisterSessionAsync(UserSession session);
}