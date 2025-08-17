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
        var result = await _gameDb.TryEquipItemAsync(userId, characterId, itemId);
        if (result.IsSuccess)
        {
            LogInfo(_logger, EventType.EquipItem, "Equip Item", new { userId, itemId });   
        }

        return result.ErrorCode;
    }

    public async Task<Result> EquipRuneAsnyc(long userId, long characterId, long runeId)
    {
        var result = await _gameDb.TryEquipRuneAsync(userId, characterId, runeId);
        if (result.IsSuccess)
        {
            LogInfo(_logger, EventType.EquipRune, "Equip Rune", new { userId, runeId });  
        }

        return result.ErrorCode;
    }

    public async Task<Result> EnhanceItemAsync(long userId, long itemId)
    {
        var result = await _gameDb.TryEnhanceItemAsync(userId, itemId);
        if (result.IsSuccess)
        {
            LogInfo(_logger, EventType.EnhanceItem,  "Enhance Item", new { userId, itemId });
        }

        return result.ErrorCode;
    }

    public async Task<Result> EnhanceRuneAsync(long userId, long itemId)
    {
        var result = await _gameDb.TryEnhanceRuneAsync(userId, itemId);
        if (result.IsSuccess)
        {
            LogInfo(_logger, EventType.EnhanceRune, "Enhance Rune", new { userId, itemId });
        }

        return result.ErrorCode;
    }
}