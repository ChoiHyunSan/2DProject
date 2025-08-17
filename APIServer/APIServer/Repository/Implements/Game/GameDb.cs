using APIServer.Config;
using Microsoft.Extensions.Options;

namespace APIServer.Repository.Implements;

partial class GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig, IMasterDb masterDb) 
    : MySQLBase(dbConfig.Value.GameDb), IGameDb
{
    private readonly IMasterDb _masterDb = masterDb;
    
    // Logger
    private readonly ILogger<GameDb> _logger = logger;
    
    // GameDb Table
    private const string TABLE_USER_GAME_DATA = "user_game_data";
    private const string TABLE_USER_INVENTORY_CHARACTER = "user_inventory_character";
    private const string TABLE_USER_INVENTORY_ITEM = "user_inventory_item";
    private const string TABLE_USER_INVENTORY_RUNE = "user_inventory_rune";
    private const string TABLE_USER_ATTENDANCE_MONTH = "user_attendance_month";
    private const string TABLE_USER_ATTENDANCE_WEEK = "user_attendance_week";
    private const string TABLE_USER_QUEST_INPROGRESS = "user_quest_inprogress";
    private const string TABLE_CHARACTER_EQUIPMENT_ITEM = "character_equipment_item";
    private const string TABLE_CHARACTER_EQUIPMENT_RUNE = "character_equipment_rune";
    
    // GameDb Table Column
    private readonly string USER_ID = "user_id";
    private readonly string ITEM_ID = "item_id";
    private readonly string RUNE_ID = "rune_id";
    private readonly string CHARACTER_ID = "character_id";
    private readonly string GOLD = "gold";
    private readonly string GEM = "gem";
    private readonly string CHARACTER_CODE = "character_code";
    private readonly string ITEM_CODE = "item_code";
    private readonly string RUNE_CODE = "rune_code";
    private readonly string LEVEL = "level";
}