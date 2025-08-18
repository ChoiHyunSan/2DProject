using APIServer.Models.Entity.Data;

namespace APIServer.Repository.Implements;

partial class MasterDb
{
    public async Task<Result<CharacterOriginData>> GetCharacterOriginDataAsync(long characterCode)
    {
        _ = _characterOriginDatas.TryGetValue(characterCode, out var characterOriginData) ? characterOriginData : null;
        if (characterOriginData == null)
        {
            return Result<CharacterOriginData>.Failure(ErrorCode.FailedGetMasterData);
        }

        return Result<CharacterOriginData>.Success(characterOriginData);
    }

    public async Task<Result<int>> GetItemSellPriceAsync(long itemCode, int level)
    {
        var enhanceData = _itemEnhanceDatas.GetValueOrDefault((itemCode, level));
        if (enhanceData == null)
        {
            return Result<int>.Failure(ErrorCode.FailedGetMasterData);
        }
        
        return Result<int>.Success(enhanceData.sellPrice);
    }

    public async Task<Result<ItemEnhanceData>> GetItemEnhanceDataAsync(long itemCode, int level)
    {
        var enhanceData = _itemEnhanceDatas.GetValueOrDefault((itemCode, level));
        if (enhanceData == null)
        {
            return Result<ItemEnhanceData>.Failure(ErrorCode.FailedGetMasterData);
        }
    
        return Result<ItemEnhanceData>.Success(enhanceData);
    }

    public async Task<Result<RuneEnhanceData>> GetRuneEnhanceDataAsync(long runeCode, int level)
    {
        var enhanceData = _runeEnhanceDatas.GetValueOrDefault((runeCode, level));
        if (enhanceData == null)
        {
            return Result<RuneEnhanceData>.Failure(ErrorCode.FailedGetMasterData);
        }

        return Result<RuneEnhanceData>.Success(enhanceData);
    }

    public async Task<Result<List<StageMonsterInfo>>> GetStageMonsterListAsync(long stageCode)
    {
        var stageMonsterInfos = _stageMonsterInfos.GetValueOrDefault(stageCode);
        if (stageMonsterInfos is { Count: 0 })
        {
            return Result<List<StageMonsterInfo>>.Failure(ErrorCode.FailedGetMasterData);
        }
        
        return Result<List<StageMonsterInfo>>.Success(stageMonsterInfos);
    }

    public Task<Result<int>> GetGoldReward(long stageCode)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> GetGemReward(long stageCode)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> GetExpReward(long stageCode)
    {
        throw new NotImplementedException();
    }
}