using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class InventoryService(ILogger<InventoryService> logger, IGameDb gameDb) 
    : IInventoryService
{
    private readonly ILogger<InventoryService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    
    public async Task<Result> EquipItemAsync(long userId, long characterId, long itemId)
    {
        if(await _gameDb.TryEquipItemAsync(userId, characterId, itemId) == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipItem, "Equip Item", new { userId, itemId });   
        }

        return ErrorCode.None;
    }

    public async Task<Result> EquipRuneAsnyc(long userId, long characterId, long runeId)
    {
        if(await _gameDb.TryEquipRuneAsync(userId, characterId, runeId) == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipRune, "Equip Rune", new { userId, runeId });  
        }

        return ErrorCode.None;
    }

    public async Task<Result> EnhanceItemAsync(long userId, long itemId)
    {
        if(await _gameDb.TryEnhanceItemAsync(userId, itemId) == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EnhanceItem,  "Enhance Item", new { userId, itemId });
        }

        return ErrorCode.None;
    }

    public async Task<Result> EnhanceRuneAsync(long userId, long itemId)
    {
        if(await _gameDb.TryEnhanceRuneAsync(userId, itemId) == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EnhanceRune, "Enhance Rune", new { userId, itemId });
        }

        return ErrorCode.None;
    }
}