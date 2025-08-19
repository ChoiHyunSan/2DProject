using APIServer.Models.DTO;

namespace APIServer.Service;

public interface IDataLoadService
{
    Task<Result<GameData>> LoadGameData(long userId);
}