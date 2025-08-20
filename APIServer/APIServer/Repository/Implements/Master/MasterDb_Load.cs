using System.Collections.Immutable;
using APIServer.Models.Entity.Data;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class MasterDb
{
    public async Task<ErrorCode> Load()
    {
        if (isAlreadyLoad) return ErrorCode.None;

        if (await LoadAttendanceAsync() == false ||
            await LoadCharacterAsync() == false ||
            await LoadItemAsync() == false ||
            await LoadRuneAsync() == false ||
            await LoadQuestAsync() == false || 
            await LoadStageAsync() == false)
        {
            Thread.Sleep(1000);
            return ErrorCode.FailedDataLoad;
        }
        
        LogInfo(_logger, EventType.LoadMasterDb, "Master Data Load Success");
        
        isAlreadyLoad = true;
        return ErrorCode.None;
    }
    
    private async Task<bool> LoadAttendanceAsync()
    {
        try
        {
            var months = await GetAllDataFromTableAsync<AttendanceRewardMonth>("attendance_reward_month");

            _attendanceRewardsMonth = months
                .GroupBy(m => m.day)
                .ToImmutableDictionary(g => g.Key, g => g.First()); 
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadAttendance, e.ToString());
            return false;
        }
        
        return true;
    }

    private async Task<bool> LoadCharacterAsync()
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
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadCharacter, e.ToString());
            return false;
        }
        
        return true;
    }

    private async Task<bool> LoadItemAsync()
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
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadItem, e.ToString());
            return false;       
        }
        
        return true;
    }
    
    private async Task<bool> LoadRuneAsync()
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
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadRune, e.ToString());
            return false;       
        }
        
        return true;
    }
    
    private async Task<bool> LoadQuestAsync()
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
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadQuest, e.ToString());
            return false;
        }

        return true;
    }
    
    private async Task<bool> LoadStageAsync()
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
                .ToImmutableDictionary(g => g.Key, g =>  g.ToList());

            _stageRewardsRune = stageRewardsRune
                .GroupBy(r => r.stageCode)
                .ToImmutableDictionary(g => g.Key, g =>  g.ToList());

            _stageMonsterInfos = stageMonsterInfos
                .GroupBy(m => m.stageCode)
                .ToImmutableDictionary(g => g.Key, g =>  g.ToList());
        }
        catch (Exception e)
        {
            LogError(_logger, ErrorCode.FailedDataLoad, EventType.LoadStage, e.ToString());
            return false;
        }
        
        return true;
    }
    
    private async Task<IEnumerable<T>> GetAllDataFromTableAsync<T>(string tableName)
    {
        return await _queryFactory.Query(tableName).GetAsync<T>();
    }
}