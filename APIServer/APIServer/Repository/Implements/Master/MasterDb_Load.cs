using System.Collections.Immutable;
using APIServer.Config;
using APIServer.Models.Entity.Data;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class MasterDb(IOptions<DbConfig> dbConfig, ILogger<MasterDb> logger)
    : MySQLBase(dbConfig.Value.MasterDb), IMasterDb
{
    // Attendance
    private ImmutableDictionary<int, AttendanceRewardMonth> _attendanceRewardsMonth = ImmutableDictionary<int, AttendanceRewardMonth>.Empty;
    private ImmutableDictionary<int, AttendanceRewardWeek> _attendanceRewardsWeek = ImmutableDictionary<int, AttendanceRewardWeek>.Empty;
    
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
    private ImmutableDictionary<long, StageRewardItem> _stageRewardsItem = ImmutableDictionary<long, StageRewardItem>.Empty;
    private ImmutableDictionary<long, StageRewardRune> _stageRewardsRune = ImmutableDictionary<long, StageRewardRune>.Empty;
    private ImmutableDictionary<long, StageMonsterInfo> _stageMonsterInfos = ImmutableDictionary<long, StageMonsterInfo>.Empty;

    private bool isAlreadyLoad = false;
    private readonly ILogger<MasterDb> _logger = logger;
    
    public async Task<bool> Load()
    {
        if (isAlreadyLoad) return true;

        if (await LoadAttendance() == false ||
            await LoadCharacter() == false ||
            await LoadItem() == false ||
            await LoadRune() == false ||
            await LoadQuest() == false || 
            await LoadStage() == false)
        {
            Thread.Sleep(1000);
            return false;
        }
        
        LogInfo(_logger, EventType.LoadMasterDb, "Master Data Load Success");
        
        isAlreadyLoad = true;
        return true;
    }
    
    private async Task<bool> LoadAttendance()
    {
        try
        {
            var months = await GetAllDataFromTableAsync<AttendanceRewardMonth>("attendance_reward_mont");
            var weeks  = await GetAllDataFromTableAsync<AttendanceRewardWeek>("attendance_reward_week");

            _attendanceRewardsMonth = months
                .GroupBy(m => m.day)
                .ToImmutableDictionary(g => g.Key, g => g.First()); 

            _attendanceRewardsWeek = weeks
                .GroupBy(w => w.day)
                .ToImmutableDictionary(g => g.Key, g => g.First());
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadAttendance, e.ToString());
            return false;
        }
        
        return true;
    }

    private async Task<bool> LoadCharacter()
    {
        try
        {
            var originDatas = await GetAllDataFromTableAsync<CharacterOriginData>("character_origin_data");
            var enhancePriceDatas = await GetAllDataFromTableAsync<CharacterEnhanceData>("character_enhance_data");
            
            _characterOriginDatas = originDatas
                .GroupBy(c => c.characterCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());
            
            _characterEnhancePriceDatas = enhancePriceDatas
                .GroupBy(c => (c.characterCode, c.level))
                .ToImmutableDictionary(g => (g.Key.characterCode, g.Key.level), g => g.First());
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadCharacter, e.ToString());
            return false;
        }
        
        return true;
    }

    private async Task<bool> LoadItem()
    {
        try
        {
            var originDatas = await GetAllDataFromTableAsync<ItemOriginData>("item_origin_data");
            var enhanceDatas = await GetAllDataFromTableAsync<ItemEnhanceData>("item_enhance_data");
            
            _itemOriginDatas = originDatas
                .GroupBy(i => i.itemCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());
            
            _itemEnhanceDatas = enhanceDatas
                .GroupBy(i => (i.itemCode, i.level))
                .ToImmutableDictionary(g => (g.Key.itemCode, g.Key.level), g => g.First());       
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadItem, e.ToString());
            return false;       
        }
        
        return true;
    }
    
    private async Task<bool> LoadRune()
    {
        try
        {
            var originDatas = await GetAllDataFromTableAsync<RuneOriginData>("rune_origin_data");
            var enhanceDatas = await GetAllDataFromTableAsync<RuneEnhanceData>("rune_enhance_data");
            
            _runeOriginDatas = originDatas
                .GroupBy(r => r.runeCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());
            
            _runeEnhanceDatas = enhanceDatas
                .GroupBy(r => (r.runeCode, r.level))
                .ToImmutableDictionary(g => (g.Key.runeCode, g.Key.level), g => g.First());      
        }
        catch(Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadRune, e.ToString());
            return false;       
        }
        
        return true;
    }
    
    private async Task<bool> LoadQuest()
    {
        try
        {
            var questInfoDatas = await GetAllDataFromTableAsync<QuestInfoData>("quest_info_data");
            _questInfoDatas = questInfoDatas
                .GroupBy(q => q.questCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());       
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadQuest, e.ToString());
            return false;
        }

        return true;
    }
    
    private async Task<bool> LoadStage()
    {
        try
        {
            var stageRewardsGold = await GetAllDataFromTableAsync<StageRewardGold>("stage_reward_gold");
            var stageRewardsItem = await GetAllDataFromTableAsync<StageRewardItem>("stage_reward_item");
            var stageRewardsRune = await GetAllDataFromTableAsync<StageRewardRune>("stage_reward_rune");
            var stageMonsterInfos = await GetAllDataFromTableAsync<StageMonsterInfo>("stage_monster_info");

            _stageRewardsGold = stageRewardsGold
                .GroupBy(g => g.stageCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());

            _stageRewardsItem = stageRewardsItem
                .GroupBy(i => i.stageCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());

            _stageRewardsRune = stageRewardsRune
                .GroupBy(r => r.stageCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());

            _stageMonsterInfos = stageMonsterInfos
                .GroupBy(m => m.stageCode)
                .ToImmutableDictionary(g => g.Key, g => g.First());
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.DataLoadFAiled, EventType.LoadStage, e.ToString());
            return false;
        }
        
        return true;
    }
    
    private async Task<IEnumerable<T>> GetAllDataFromTableAsync<T>(string tableName)
    {
        return await _queryFactory.Query(tableName).GetAsync<T>();
    }
}