using APIServer.Models.DTO;
using APIServer.Models.DTO.Quest;
using APIServer.Models.Entity;

namespace APIServer.Service;

public interface IDataLoadService
{
    Task<Result<GameData>> LoadGameData(long userId);
    Task<Result<List<UserQuestInprogress>>> GetProgressQuestList(long userId, string email, Pageable pageable);
    Task<Result<List<UserQuestComplete>>> GetCompleteQuestList(long userId, Pageable pageable);
}