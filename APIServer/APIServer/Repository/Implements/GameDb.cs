using APIServer.Config;
using APIServer.Models.Entity;
using Microsoft.Extensions.Options;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

public class GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig) 
    : MySQLBase(dbConfig.Value.GameDb),IGameDb
{
    private readonly ILogger<GameDb> _logger = logger;

    public async Task<UserGameData> TestInsert()
    {
        var userId = await _queryFactory.Query("user_game_data").InsertGetIdAsync<long>(new
        {
            gold = 10000,
            gem = 0,
            exp = 0,
            level = 1,
            total_monster_kill_count = 0,
            total_clear_count = 0
        });
        
        var userData = await _queryFactory.Query("user_game_data")
            .Where("user_id", userId)
            .FirstAsync<UserGameData>();

        return userData;
    }
}