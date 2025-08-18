using System.Collections.Immutable;
using APIServer.Config;
using APIServer.Models.Entity.Data;
using Microsoft.Extensions.Options;

namespace APIServer.Repository.Implements;

partial class MasterDb(IOptions<DbConfig> dbConfig, ILogger<MasterDb> logger)
    : MySQLBase(dbConfig.Value.MasterDb), IMasterDb
{
    // Flag
    private bool isAlreadyLoad = false;
    
    // Logger
    private readonly ILogger<MasterDb> _logger = logger;
    
    // Attendance
    public ImmutableDictionary<int, AttendanceRewardMonth> _attendanceRewardsMonth = ImmutableDictionary<int, AttendanceRewardMonth>.Empty;
    public ImmutableDictionary<int, AttendanceRewardWeek> _attendanceRewardsWeek = ImmutableDictionary<int, AttendanceRewardWeek>.Empty;
    
    // Character
    public ImmutableDictionary<long, CharacterOriginData> _characterOriginDatas = ImmutableDictionary<long, CharacterOriginData>.Empty;
    public ImmutableDictionary<(long, int), CharacterEnhanceData> _characterEnhancePriceDatas = ImmutableDictionary<(long, int), CharacterEnhanceData>.Empty;
    
    // Item
    public ImmutableDictionary<long, ItemOriginData> _itemOriginDatas = ImmutableDictionary<long, ItemOriginData>.Empty;
    public ImmutableDictionary<(long, int), ItemEnhanceData> _itemEnhanceDatas = ImmutableDictionary<(long, int), ItemEnhanceData>.Empty;
    
    // Rune
    public ImmutableDictionary<long, RuneOriginData> _runeOriginDatas = ImmutableDictionary<long, RuneOriginData>.Empty;
    public ImmutableDictionary<(long, int), RuneEnhanceData> _runeEnhanceDatas = ImmutableDictionary<(long, int), RuneEnhanceData>.Empty;
    
    // Quest
    public ImmutableDictionary<long, QuestInfoData> _questInfoDatas = ImmutableDictionary<long, QuestInfoData>.Empty;

    // Stage
    public ImmutableDictionary<long, StageRewardGold> _stageRewardsGold = ImmutableDictionary<long, StageRewardGold>.Empty;
    public ImmutableDictionary<long, StageRewardItem> _stageRewardsItem = ImmutableDictionary<long, StageRewardItem>.Empty;
    public ImmutableDictionary<long, StageRewardRune> _stageRewardsRune = ImmutableDictionary<long, StageRewardRune>.Empty;
    public ImmutableDictionary<long, List<StageMonsterInfo>> _stageMonsterInfos = ImmutableDictionary<long, List<StageMonsterInfo>>.Empty;
}