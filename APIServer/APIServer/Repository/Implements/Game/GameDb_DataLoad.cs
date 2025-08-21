using APIServer.Models.DTO;
using APIServer.Models.Entity;
using Dapper;
using MySqlConnector;
using SqlKata.Execution;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements;

partial class GameDb
{
        public async Task<GameData> GetAllGameDataByUserIdAsync(long userId)
    {
         var sql = @"
            SELECT gold, gem, exp, level,
                   total_monster_kill_count AS totalMonsterKillCount,
                   total_clear_count        AS totalClearCount
            FROM user_game_data
            WHERE user_id = @userId;

            SELECT character_id, character_code AS characterCode, level
            FROM user_inventory_character
            WHERE user_id = @userId;

            SELECT item_id, item_code AS itemCode, level
            FROM user_inventory_item
            WHERE user_id = @userId;

            SELECT rune_id, rune_code AS runeCode, level
            FROM user_inventory_rune
            WHERE user_id = @userId;

            -- 장착 아이템(캐릭터별)
            SELECT cei.character_id, i.item_id AS itemId
            FROM character_equipment_item cei
            JOIN user_inventory_item i ON i.item_id = cei.item_id
            WHERE i.user_id = @userId;

            -- 장착 룬(캐릭터별)
            SELECT cer.character_id, r.rune_id AS runeId
            FROM character_equipment_rune cer
            JOIN user_inventory_rune r ON r.rune_id = cer.rune_id
            WHERE r.user_id = @userId;

            -- 진행 중 퀘스트
            SELECT quest_code AS questCode, progress, expire_date AS expireDate
            FROM user_quest_inprogress
            WHERE user_id = @userId;

            -- 클리어 스테이지
            SELECT stage_Code AS stageCode
            FROM user_clear_stage
            WHERE user_id = @userId;
            ";

         try
         { 
            // SqlKata의 커넥션 재사용
            var conn = (MySqlConnection)_queryFactory.Connection;

            using var multi = await conn.QueryMultipleAsync(sql, new { userId });

            // 1) 기본 스탯
            var baseRow = await multi.ReadFirstOrDefaultAsync<(int gold, int gem, int exp, int level, int totalMonsterKillCount, int totalClearCount)>();
            if (baseRow.Equals(default)) return null;

            // 2) 캐릭터
            var characterRows = (await multi.ReadAsync<(long character_id, long characterCode, int level)>()).ToList();

            // 3) 인벤토리 아이템/룬
            var itemRows = (await multi.ReadAsync<(long item_id, long itemCode, int level)>()).ToList();
            var runeRows = (await multi.ReadAsync<(long rune_id, long runeCode, int level)>()).ToList();

            // 4) 장착 목록(캐릭터별)
            var equipItemRows = (await multi.ReadAsync<(long character_id, long itemId)>()).ToList();
            var equipRuneRows = (await multi.ReadAsync<(long character_id, long runeId)>()).ToList();

            // 5) 퀘스트 / 스테이지
            var quests = (await multi.ReadAsync<QuestData>()).ToList();
            var clearStages = (await multi.ReadAsync<ClearStageData>()).ToList();

            // GameData 조립
            var gameData = new GameData
            {
                gold = baseRow.gold,
                gem = baseRow.gem,
                exp = baseRow.exp,
                level = baseRow.level,
                totalMonsterKillCount = baseRow.totalMonsterKillCount,
                totalClearCount = baseRow.totalClearCount,

                items = itemRows.Select(x => new ItemData
                {
                    itemId = x.item_id,
                    itemCode = x.itemCode,
                    level = x.level
                }).ToList(),

                runes = runeRows.Select(x => new RuneData
                {
                    runeId = x.rune_id,
                    runeCode = x.runeCode,
                    level = x.level
                }).ToList(),

                quests = quests,
                clearStages = clearStages
            };

            // 캐릭터 + 장착 정보 조립
            var equipItemsByChar = equipItemRows.GroupBy(x => x.character_id)
                .ToDictionary(g => g.Key, g => g.Select(e => new EquipItemData() { itemId = e.itemId }).ToList());

            var equipRunesByChar = equipRuneRows.GroupBy(x => x.character_id)
                .ToDictionary(g => g.Key, g => g.Select(e => new EquipRuneData() { runeId = e.runeId }).ToList());

            gameData.characters = characterRows.Select(c => new CharacterData
            {
                characterId = c.character_id,
                characterCode = c.characterCode,
                level = c.level,
                equipItems = equipItemsByChar.TryGetValue(c.character_id, out var equipItems) ? equipItems : [],
                equipRunes = equipRunesByChar.TryGetValue(c.character_id, out var equipRunes) ? equipRunes : []
            }).ToList();

            LogInfo(_logger, EventType.LoadGameDb, "GetAllGameDataByUserIdAsync", new
            {
                userId
            });
            
            return gameData;
         }
         catch (Exception e)
         {
             LogError(_logger, ErrorCode.FailedLoadAllGameData, EventType.LoadGameDb, "GetAllGameDataByUserIdAsync Failed", new
             {
                 userId,
                 trace = e.StackTrace,
                 message = e.Message,
             });
             
             return null;
         } 
    }

    public async Task<List<UserQuestInprogress>> GetProgressQuestList(long userId)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_INPROGRESS)
            .Where(USER_ID, userId)
            .GetAsync<UserQuestInprogress>();

        return result.ToList();
    }

    public async Task<List<UserQuestComplete>> GetCompleteQuestList(long userId, Pageable pageable)
    {
        var offset = (pageable.page - 1) * pageable.size;
        
        var result = await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .Limit(pageable.size)
            .Offset(offset)
            .GetAsync<UserQuestComplete>();

        return result.ToList();
    }

    public async Task<UserQuestComplete> GetCompleteQuest(long userId, long questCode)
    {
        return await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .Where(QUEST_CODE, questCode)
            .FirstOrDefaultAsync<UserQuestComplete>();
    }

    public async Task<bool> RewardCompleteQuest(long userId, long questCode)
    {
        var result = await _queryFactory.Query(TABLE_USER_QUEST_COMPLETED)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                EARN_REWARD = true
            });

        return result == 1;
    }
}