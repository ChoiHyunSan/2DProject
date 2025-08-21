using APIServer.Models.DTO;
using APIServer.Models.DTO.Quest;
using APIServer.Models.Entity;
using APIServer.Repository;
using ZLogger;
using static APIServer.LoggerManager;
using static APIServer.Models.DTO.Pageable;

namespace APIServer.Service.Implements;

public class DataLoadService(ILogger<DataLoadService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    : IDataLoadService
{
    private readonly ILogger<DataLoadService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
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

    public async Task<Result<List<UserQuestInprogress>>> GetProgressQuestList(long userId, string email, Pageable pageable)
    {
        try
        {
            // 캐시된 퀘스트 리스트가 있는지 확인
            var cache = await _memoryDb.GetCachedQuestList(email);
            if (cache.IsSuccess)
            {
                return Result<List<UserQuestInprogress>>.Success(Pagination(cache.Value, pageable));
            }

            // 없다면 GameDB에서 가져옴
            var progressList = await _gameDb.GetProgressQuestList(userId);

            // 퀘스트 리스트를 캐싱
            if (await _memoryDb.CacheQuestList(email, progressList) is var cacheQuest && cacheQuest.IsFailed)
            {
                return Result<List<UserQuestInprogress>>.Failure(cacheQuest.ErrorCode);
            }

            LogInfo(_logger, EventType.GetProgressQuest, "Get Progress Quest List", new { userId, email });
            
            // 데이터 반환
            return Result<List<UserQuestInprogress>>.Success(Pagination(progressList, pageable));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetProgressQuest, 
                "Failed Get Progress Quest List", new { userId, ex.Message, ex.StackTrace });;
            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.FailedDataLoad);       
        }
    }

    public async Task<Result<List<UserQuestComplete>>> GetCompleteQuestList(long userId, Pageable pageable)
    {
        try
        {
            var completeList = await _gameDb.GetCompleteQuestList(userId, pageable);
            
            LogInfo(_logger, EventType.GetCompleteQuest, "Get Complete Quest List", new { userId });
            
            return Result<List<UserQuestComplete>>.Success(completeList);
        }catch(Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetCompleteQuest, 
                "Failed Get Complete Quest List", new { userId, ex.Message, ex.StackTrace });;
            return Result<List<UserQuestComplete>>.Failure(ErrorCode.FailedDataLoad);       
        }
    }
}