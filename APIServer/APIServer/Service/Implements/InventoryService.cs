using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class InventoryService(ILogger<InventoryService> logger, IGameDb gameDb) 
    : IInventoryService
{
    private readonly ILogger<InventoryService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    
    public async Task<ErrorCode> EquipItem(long userId, long characterId, long itemId)
    {
        var result = await _gameDb.TryEquipItem(userId, characterId, itemId);
        if (result == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipItem, "Equip Item", new { userId, itemId });   
        }

        return result;
    }

    public async Task<ErrorCode> EquipRune(long userId, long characterId, long runeId)
    {
        var result = await _gameDb.TryEquipRune(userId, characterId, runeId);
        if (result == ErrorCode.None)
        {
            LogInfo(_logger, EventType.EquipRune, "Equip Rune", new { userId, runeId });  
        }

        return result;
    }
}