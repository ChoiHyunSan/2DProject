using APIServer.Models.DTO;
using APIServer.Repository;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class DataLoadService(ILogger<DataLoadService> logger, IGameDb gameDb)
    : IDataLoadService
{
    private readonly ILogger<DataLoadService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    
    public async Task<Result<GameData>> LoadGameData(long userId)
    {
        try
        {
            // 게임데이터 조회
            var gameData = await _gameDb.GetAllGameDataByUserIdAsync(userId);

            LogInfo(_logger, EventType.LoadGameData, "Load Game Data", new { userId });
            return Result<GameData>.Success(gameData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadGameData, 
                "Failed Load Game Data", new { userId, ex.Message, ex.StackTrace });;
            return Result<GameData>.Failure(ErrorCode.FailedDataLoad);
        }
    }
}