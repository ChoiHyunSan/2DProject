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
    
    public async Task<Result<FullGameData>> LoadGameDataAsync(long userId)
    {
        try
        {
            // 게임데이터 조회
            var gameData = await _gameDb.GetAllGameDataByUserIdAsync(userId);

            LogInfo(_logger, EventType.LoadGameData, "Load Game Data", new { userId });
            return Result<FullGameData>.Success(gameData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadGameData, 
                "Failed Load Game Data", new { userId, ex.Message, ex.StackTrace });;
            return Result<FullGameData>.Failure(ErrorCode.FailedDataLoad);
        }
    }

    public async Task<Result<List<UserQuestInprogress>>> GetProgressQuestListAsync(long userId, Pageable pageable)
    {
        try
        {
            // 캐시된 퀘스트 리스트가 있는지 확인
            var cache = await _memoryDb.GetCachedQuestList(userId);
            if (cache.IsSuccess)
            {
                return Result<List<UserQuestInprogress>>.Success(Pagination(cache.Value, pageable));
            }

            // 없다면 GameDB에서 가져옴
            var progressList = await _gameDb.GetProgressQuestList(userId);

            // 퀘스트 리스트를 캐싱
            if (await _memoryDb.CacheQuestList(userId, progressList) is var cacheQuest && cacheQuest.IsFailed)
            {
                return Result<List<UserQuestInprogress>>.Failure(cacheQuest.ErrorCode);
            }

            LogInfo(_logger, EventType.GetProgressQuest, "Get Progress Quest List", new { userId });
            
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

    public async Task<Result<List<UserQuestComplete>>> GetCompleteQuestListAsync(long userId, Pageable pageable)
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

    public async Task<Result<List<CharacterData>>> GetInventoryCharacterListAsync(long userId)
    {
        try
        {
            // 캐시 데이터 조회
            if (await _memoryDb.GetCachedCharacterDataList(userId) is var load && load.IsSuccess)
            {
                return Result<List<CharacterData>>.Success(load.Value);
            }
            
            // 없는 경우 DB 조회
            var characterDataList = await _gameDb.GetCharacterDataListAsync(userId);
            
            LogInfo(_logger, EventType.GetInventoryCharacter, "Get Inventory Character List", new { userId });
            
            // 데이터 캐싱
            if (await _memoryDb.CacheCharacterDataList(userId, characterDataList)  == false)
            {
                return Result<List<CharacterData>>.Failure(ErrorCode.FailedCacheGameData);
            }
            
            return Result<List<CharacterData>>.Success(characterDataList);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetInventoryCharacter, 
                "Failed Get Inventory Chracter List", new { userId });
            
            return Result<List<CharacterData>>.Failure(ErrorCode.FailedDataLoad);       
        }
    }

    public async Task<Result<List<ItemData>>> GetInventoryItemListAsync(long userId, Pageable requestPageable)
    {
        try
        {
            // 캐시 데이터 조회
            if (await _memoryDb.GetCachedItemDataList(userId) is var load && load.IsSuccess)
            {
                return Result<List<ItemData>>.Success(load.Value);
            }
            
            var itemDataList = await _gameDb.GetItemDataListAsync(userId, requestPageable);

            LogInfo(_logger, EventType.GetInventoryItem, "Get Inventory Item List", new { sessionUserId = userId });

            // 데이터 캐싱
            if (await _memoryDb.CacheItemDataList(userId, itemDataList) == false)
            {
                return Result<List<ItemData>>.Failure(ErrorCode.FailedCacheGameData);
            }
            
            return Result<List<ItemData>>.Success(itemDataList);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetInventoryItem, 
                "Failed Get Inventory Item List", new { userId });
            
            return Result<List<ItemData>>.Failure(ErrorCode.FailedDataLoad);      
        }
    }

    public async Task<Result<List<RuneData>>> GetInventoryRuneListAsync(long userId, Pageable requestPageable)
    {
        try
        {
            // 캐시 데이터 조회
            if (await _memoryDb.GetCachedRuneDataList(userId) is var load && load.IsSuccess)
            {
                return Result<List<RuneData>>.Success(load.Value);
            }
            
            var runeDataList = await _gameDb.GetRuneDataListAsync(userId, requestPageable);

            LogInfo(_logger, EventType.GetInventoryRune, "Get Inventory Rune List", new { sessionUserId = userId });

            // 데이터 캐싱
            if (await _memoryDb.CacheRuneDataList(userId, runeDataList) == false)
            {
                return Result<List<RuneData>>.Failure(ErrorCode.FailedCacheGameData);
            }
            
            return Result<List<RuneData>>.Success(runeDataList);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetInventoryRune, 
                "Failed Get Inventory Rune List", new { userId });
            
            return Result<List<RuneData>>.Failure(ErrorCode.FailedDataLoad);      
        }
    }

    public async Task<Result<UserGameData>> GetUserGameDataAsync(long userId)
    {
        try
        {
            // 캐시 데이터 조회
            if (await _memoryDb.GetCachedUserGameData(userId) is var load && load.IsSuccess)
            {
                return Result<UserGameData>.Success(load.Value);
            }
            
            var gameData = await _gameDb.GetUserGameDataAsync(userId);
            
            // 데이터 캐싱
            if (await _memoryDb.CacheUserGameData(userId, gameData) == false)
            {
                return Result<UserGameData>.Failure(ErrorCode.FailedCacheGameData);
            }
            
            LogInfo(_logger, EventType.GetInventoryRune, "Get User Game Data", new { sessionUserId = userId });;
            
            return Result<UserGameData>.Success(gameData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.GetInventoryRune, 
                "Failed Get User Game Data", new { userId });
            
            return Result<UserGameData>.Failure(ErrorCode.FailedDataLoad);
        }
    }
}