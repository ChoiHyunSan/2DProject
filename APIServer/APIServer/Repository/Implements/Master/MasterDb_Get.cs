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
}