using System.Collections.Immutable;
using APIServer.Models.Entity.Data;

namespace APIServer.Repository;

public interface IMasterDb
{
    /// <summary> Master Db 데이터 로드 </summary>
    public Task<ErrorCode> Load();
    
    ImmutableDictionary<int, AttendanceRewardMonth> GetAttendanceRewardMonths();
    ImmutableDictionary<long, CharacterOriginData> GetCharacterOriginDatas();
    ImmutableDictionary<(long, int), CharacterEnhanceData> GetCharacterEnhancePriceDatas();
    ImmutableDictionary<long, ItemOriginData> GetItemOriginDatas();
    ImmutableDictionary<(long, int), ItemEnhanceData> GetItemEnhanceDatas();
    ImmutableDictionary<long, RuneOriginData> GetRuneOriginDatas();
    ImmutableDictionary<(long, int), RuneEnhanceData> GetRuneEnhanceDatas();
    ImmutableDictionary<long, QuestInfoData> GetQuestInfoDatas();
    ImmutableDictionary<long, StageRewardGold> GetStageRewardsGold();
    ImmutableDictionary<long, List<StageRewardItem>> GetStageRewardsItem();
    ImmutableDictionary<long, List<StageRewardRune>> GetStageRewardsRune();
    ImmutableDictionary<long, List<StageMonsterInfo>> GetStageMonsterList();
}
