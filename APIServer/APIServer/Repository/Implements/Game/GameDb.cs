using System.Transactions;
using APIServer.Config;
using APIServer.Models.Entity;
using Microsoft.Extensions.Options;
using SqlKata.Execution;
using IsolationLevel = System.Data.IsolationLevel;

namespace APIServer.Repository.Implements;

partial class GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig, IMasterDb masterDb) 
    : MySQLBase(dbConfig.Value.GameDb), IGameDb
{
    private readonly IMasterDb _masterDb = masterDb;
    private readonly ILogger<GameDb> _logger = logger;
    
    // GameDb Table
    private const string TABLE_USER_GAME_DATA = "user_game_data";
    private const string TABLE_USER_INVENTORY_CHARACTER = "user_inventory_character";
    private const string TABLE_USER_INVENTORY_ITEM = "user_inventory_item";
    private const string TABLE_USER_INVENTORY_RUNE = "user_inventory_rune";
    private const string TABLE_USER_ATTENDANCE_MONTH = "user_attendance_month";
    private const string TABLE_USER_ATTENDANCE_WEEK = "user_attendance_week";
    private const string TABLE_USER_QUEST_INPROGRESS = "user_quest_inprogress";
    private const string TABLE_USER_QUEST_COMPLETED = "user_quest_completed";
    private const string TABLE_CHARACTER_EQUIPMENT_ITEM = "character_equipment_item";
    private const string TABLE_CHARACTER_EQUIPMENT_RUNE = "character_equipment_rune";
    private const string TABLE_USER_CLEAR_STAGE = "user_clear_stage";
    private const string TABLE_USER_MAIL = "user_mail";
    
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
    private readonly string STAGE_CODE = "stage_code";
    private readonly string QUEST_CODE = "quest_code";
    private readonly string QUEST_TYPE = "quest_type";
    private readonly string MAIL_ID = "mail_id";
    private readonly string RECEIVE_DATE = "receive_date";
    
    // 비동기, 반환값 없음
    public async Task<ErrorCode> WithTransactionAsync(
        Func<QueryFactory, Task<ErrorCode>> action)
    {
        var txOptions = new TransactionOptions
        {
            IsolationLevel = MapIsolation(IsolationLevel.ReadCommitted),
            Timeout = TransactionManager.DefaultTimeout
        };

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            txOptions,
            TransactionScopeAsyncFlowOption.Enabled);
        
        EnsureOpen();
        _conn.EnlistTransaction(Transaction.Current!); 

        var ec = await action(_queryFactory);

        if (ec == ErrorCode.None)
            scope.Complete(); 

        return ec;
    }

    // 비동기, 반환값 있음
    public async Task<TResult> WithTransactionAsync<TResult>(
        Func<QueryFactory, Task<TResult>> func)
    {
        EnsureOpen();

        var txOptions = new TransactionOptions
        {
            IsolationLevel = MapIsolation(IsolationLevel.ReadCommitted),
            Timeout = TransactionManager.DefaultTimeout
        };

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            txOptions,
            TransactionScopeAsyncFlowOption.Enabled);
        var result = await func(_queryFactory);
        scope.Complete();
        return result;
    }
}