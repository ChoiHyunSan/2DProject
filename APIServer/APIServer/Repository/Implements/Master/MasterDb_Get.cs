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

    public async Task<Result<ItemEnhanceData>> GetItemEnhanceData(long itemCode, int level)
    {
        var enhanceData = _itemEnhanceDatas.GetValueOrDefault((itemCode, level));
        if (enhanceData == null)
        {
            return Result<ItemEnhanceData>.Failure(ErrorCode.FailedGetMasterData);
        }
    
        return Result<ItemEnhanceData>.Success(enhanceData);
    }

    public async Task<Result<RuneEnhanceData>> GetRuneEnhanceData(long runeCode, int level)
    {
        var enhanceData = _runeEnhanceDatas.GetValueOrDefault((runeCode, level));
        if (enhanceData == null)
        {
            return Result<RuneEnhanceData>.Failure(ErrorCode.FailedGetMasterData);
        }

        return Result<RuneEnhanceData>.Success(enhanceData);
    }
}