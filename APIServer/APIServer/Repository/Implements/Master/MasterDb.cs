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
    private ImmutableDictionary<int, AttendanceRewardMonth> _attendanceRewardsMonth = ImmutableDictionary<int, AttendanceRewardMonth>.Empty;
    
    // Character
    private ImmutableDictionary<long, CharacterOriginData> _characterOriginDatas = ImmutableDictionary<long, CharacterOriginData>.Empty;
    private ImmutableDictionary<(long, int), CharacterEnhanceData> _characterEnhancePriceDatas = ImmutableDictionary<(long, int), CharacterEnhanceData>.Empty;
    
    // Item
    private ImmutableDictionary<long, ItemOriginData> _itemOriginDatas = ImmutableDictionary<long, ItemOriginData>.Empty;
    private ImmutableDictionary<(long, int), ItemEnhanceData> _itemEnhanceDatas = ImmutableDictionary<(long, int), ItemEnhanceData>.Empty;
    
    // Rune
    private ImmutableDictionary<long, RuneOriginData> _runeOriginDatas = ImmutableDictionary<long, RuneOriginData>.Empty;
    private ImmutableDictionary<(long, int), RuneEnhanceData> _runeEnhanceDatas = ImmutableDictionary<(long, int), RuneEnhanceData>.Empty;
    
    // Quest
    private ImmutableDictionary<long, QuestInfoData> _questInfoDatas = ImmutableDictionary<long, QuestInfoData>.Empty;

    // Stage
    private ImmutableDictionary<long, StageRewardGold> _stageRewardsGold = ImmutableDictionary<long, StageRewardGold>.Empty;
    private ImmutableDictionary<long, List<StageRewardItem>> _stageRewardsItem = ImmutableDictionary<long, List<StageRewardItem>>.Empty;
    private ImmutableDictionary<long, List<StageRewardRune>> _stageRewardsRune = ImmutableDictionary<long, List<StageRewardRune>>.Empty;
    private ImmutableDictionary<long, List<StageMonsterInfo>> _stageMonsterInfos = ImmutableDictionary<long, List<StageMonsterInfo>>.Empty;
    
    
    public ImmutableDictionary<int, AttendanceRewardMonth> GetAttendanceRewardMonths() => _attendanceRewardsMonth;
    public ImmutableDictionary<long, CharacterOriginData> GetCharacterOriginDatas() => _characterOriginDatas;
    public ImmutableDictionary<(long, int), CharacterEnhanceData> GetCharacterEnhanceDatas() => _characterEnhancePriceDatas;
    public ImmutableDictionary<long, ItemOriginData> GetItemOriginDatas() => _itemOriginDatas;
    public ImmutableDictionary<(long, int), ItemEnhanceData> GetItemEnhanceDatas() => _itemEnhanceDatas;
    public ImmutableDictionary<long, RuneOriginData> GetRuneOriginDatas() => _runeOriginDatas;
    public ImmutableDictionary<(long, int), RuneEnhanceData> GetRuneEnhanceDatas() =>  _runeEnhanceDatas;
    public ImmutableDictionary<long, QuestInfoData> GetQuestInfoDatas() => _questInfoDatas;
    public ImmutableDictionary<long, StageRewardGold> GetStageRewardsGold() => _stageRewardsGold;
    public ImmutableDictionary<long, List<StageRewardItem>> GetStageRewardsItem() => _stageRewardsItem;
    public ImmutableDictionary<long, List<StageRewardRune>> GetStageRewardsRune() => _stageRewardsRune;
    public ImmutableDictionary<long, List<StageMonsterInfo>> GetStageMonsterList() =>  _stageMonsterInfos;
}