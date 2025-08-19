using APIServer.Models.Entity.Data;
using APIServer.Repository;
using static APIServer.LoggerManager;

namespace APIServer.Service.Implements;

public class InventoryService(ILogger<InventoryService> logger, IGameDb gameDb, IMasterDb masterDb) 
    : IInventoryService
{
    private readonly ILogger<InventoryService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMasterDb _masterDb = masterDb;
    
    public async Task<Result> EquipItemAsync(long userId, long characterId, long itemId)
    {
        try
        {
            // 캐릭터 ID에 대한 소유 여부 확인
            if (await _gameDb.IsCharacterExistsAsync(userId, characterId) == false)
            {
                return ErrorCode.CannotFindCharacter;
            }

            // 아이템 보유 여부 확인
            if (await _gameDb.IsItemExistsAsync(userId, itemId) == false)
            {
                return ErrorCode.CannotFindInventoryItem;
            }

            // 아이템 장착 여부 확인
            if (await _gameDb.IsItemEquippedAsync(itemId))
            {
                return ErrorCode.AlreadyEquippedItem;
            }

            // 아이템 장착
            if (await _gameDb.EquipItemAsync(characterId, itemId) == false)
            {
                return ErrorCode.FailedEquipItem;
            }

            LogInfo(_logger, EventType.EquipItem, "Equip Item", new { userId, itemId });

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedEquipItem, EventType.EquipItem, "Failed Equip Item", new { userId, itemId, ex.Message, ex.StackTrace});
            return ErrorCode.FailedEquipItem;
        }
    }

    public async Task<Result> EquipRuneAsnyc(long userId, long characterId, long runeId)
    {
        try
        {
            // 캐릭터 ID에 대한 소유 여부 확인
            if (await _gameDb.IsCharacterExistsAsync(userId, characterId) == false)
            {
                return ErrorCode.CannotFindCharacter;
            }

            // 룬 보유 여부 확인
            if (await _gameDb.IsRuneExistsAsync(userId, runeId) == false)
            {
                return ErrorCode.CannotFindInventoryRune;
            }

            // 룬 장착 여부 확인
            if (await _gameDb.IsRuneEquippedAsync(runeId))
            {
                return ErrorCode.AlreadyEquippedRune;
            }

            // 룬 장착
            if (await _gameDb.EquipRuneAsync(characterId, runeId) == false)
            {
                return ErrorCode.FailedEquipRune;
            }

            LogInfo(_logger, EventType.EquipRune, "Equip Rune", new { userId, runeId });

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedEquipRune, EventType.EquipRune, "Failed Equip Rune", new { userId, runeId , ex.Message, ex.StackTrace});
            return ErrorCode.FailedEquipRune;
        }
    }

    public async Task<Result> EnhanceItemAsync(long userId, long itemId)
    {
        try
        {
            // 강화에 필요한 데이터 조회
            var item = await _gameDb.GetInventoryItemAsync(userId, itemId);
            var (gold, gem) = await _gameDb.GetUserCurrencyAsync(userId);
            var enhanceData = _masterDb.GetItemEnhanceDatas()[(item.itemCode, item.level)];

            // 강화 가능 여부 조회
            if (VerifyEnhanceItem(enhanceData, gold) is var verify && verify != ErrorCode.None)
            {
                return verify;
            }

            // 갱신용 재화 정보 계산
            var (newLevel, newGold) = (enhanceData.level + 1, gold - enhanceData.enhancePrice);

            // 트랜잭션 처리
            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 아이템 레벨 갱신
                if (await _gameDb.UpdateItemLevelAsync(userId, itemId, newLevel) == false)
                {
                    return ErrorCode.FailedUpdateData;
                }

                // 유저 재화 갱신
                if (await _gameDb.UpdateUserCurrencyAsync(userId, newGold, gem) == false)
                {
                    return ErrorCode.FailedUpdateGoldAndGem;
                }

                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return txErrorCode;
            }

            LogInfo(_logger, EventType.EnhanceItem, "Enhance Item", new { userId, itemId });

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedEnhanceItem, EventType.EnhanceItem, "Failed Enhance Item", new { userId, itemId , ex.Message, ex.StackTrace});
            return ErrorCode.FailedEnhanceItem;
        }
    }

    public async Task<Result> EnhanceRuneAsync(long userId, long runeId)
    {
        try
        {
            // 강화에 필요한 데이터 조회
            var rune = await _gameDb.GetInventoryRuneAsync(userId, runeId);
            var (gold, gem) = await _gameDb.GetUserCurrencyAsync(userId);
            var enhanceData = _masterDb.GetRuneEnhanceDatas()[(rune.runeCode, rune.level)];

            // 강화 가능 여부 확인
            if (VerifyEnhanceRune(enhanceData, gold) is var verify && verify != ErrorCode.None)
            {
                return verify;
            }

            // 갱신용 재화 정보 계산
            var (newLevel, newGold) = (enhanceData.level + 1, gold - enhanceData.enhanceCount);

            // 트랜잭션 처리 
            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 룬 레벨 갱신
                if (await _gameDb.UpdateRuneLevelAsync(userId, runeId, newLevel) == false)
                {
                    return ErrorCode.FailedUpdateData;
                }

                // 유저 재화 갱신
                if (await _gameDb.UpdateUserCurrencyAsync(userId, newGold, gem) == false)
                {
                    return ErrorCode.FailedUpdateGoldAndGem;
                }

                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return txErrorCode;
            }

            LogInfo(_logger, EventType.EnhanceRune, "Enhance Rune", new { userId, itemId = runeId });

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedEnhanceRune, EventType.EnhanceRune, "Failed Enhance Rune", new { userId, runeId, ex.Message, ex.StackTrace});
            return ErrorCode.FailedEnhanceRune;       
        }
    }
    
    private ErrorCode VerifyEnhanceItem(ItemEnhanceData enhanceData, int gold)
    {
        if (enhanceData.level >= 3)
        {
            return ErrorCode.AlreadyMaximumLevelItem;
        }

        if (enhanceData.enhancePrice > gold)
        {
            return ErrorCode.GoldShortage;
        }

        return ErrorCode.None;
    }

    private ErrorCode VerifyEnhanceRune(RuneEnhanceData enhanceData, int gold)
    {
        if (enhanceData.level >= 3)
        {
            return ErrorCode.AlreadyMaximumLevelRune;
        }

        if (enhanceData.enhanceCount > gold)
        {
            return ErrorCode.GoldShortage;
        }

        return ErrorCode.None;
    }
}