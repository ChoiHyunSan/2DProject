using APIServer.Config;
using Microsoft.Extensions.Options;

namespace APIServer.Repository.Implements;

partial class GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig) 
    : MySQLBase(dbConfig.Value.GameDb),IGameDb
{
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

    // GameDb Table Column
    public readonly string USER_ID = "user_id";
}