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
}