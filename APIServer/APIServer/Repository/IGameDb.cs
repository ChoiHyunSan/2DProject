using APIServer.Models.Entity;

namespace APIServer.Repository;

public interface IGameDb
{
    Task<UserGameData> TestInsert();
}