using APIServer.Models.Entity.Data;

namespace APIServer.Repository.Implements;

partial class MasterDb
{
    public async Task<(ErrorCode, CharacterOriginData)> GetCharacterOriginDataAsync(long characterCode)
    {
        _ = _characterOriginDatas.TryGetValue(characterCode, out var characterOriginData) ? characterOriginData : null;
        if (characterOriginData == null)
        {
            return (ErrorCode.FailedGetMasterData, new CharacterOriginData());
        }

        return (ErrorCode.None, characterOriginData);
    }

    public async Task<(ErrorCode, int)> GetItemSellPriceAsync(long itemCode, int level)
    {
        var enhanceData = _itemEnhanceDatas.GetValueOrDefault((itemCode, level));
        if (enhanceData == null)
        {
            return (ErrorCode.FailedGetMasterData, 0);
        }

        return (ErrorCode.None, enhanceData.sellPrice);
    }

    public async Task<(ErrorCode, ItemEnhanceData)> GetItemEnhanceData(long itemCode, int level)
    {
        var enhanceData = _itemEnhanceDatas.GetValueOrDefault((itemCode, level));
        if (enhanceData == null)
        {
            return (ErrorCode.FailedGetMasterData, new ItemEnhanceData());
        }

        return (ErrorCode.None, enhanceData);
    }

    public async Task<(ErrorCode, RuneEnhanceData)> GetRuneEnhanceData(long runeCode, int level)
    {
        var enhanceData = _runeEnhanceDatas.GetValueOrDefault((runeCode, level));
        if (enhanceData == null)
        {
            return (ErrorCode.FailedGetMasterData, new RuneEnhanceData());
        }

        return (ErrorCode.None, enhanceData);
    }
}