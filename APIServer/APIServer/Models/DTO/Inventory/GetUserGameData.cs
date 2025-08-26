using APIServer.Models.Entity;

namespace APIServer.Models.DTO.Inventory;

public class GetUserGameDataRequest : RequestBase
{
    
}

public class GetUserGameDataResponse : ResponseBase
{
    public GameData gameData { get; set; } = new();
}

public class GameData
{
    public int gold { get; set; }
    public int gem { get; set; }
    public int exp { get; set; }
    public int level { get; set; }
    public int totalMonsterKillCount { get; set; }
    public int totalClearCount { get; set; }

    public static GameData Of(UserGameData? value)
    {
        return new GameData
        {
            gold = value.gold,
            gem = value.gem,
            exp = value.exp,
            level = value.level,
            totalMonsterKillCount = value.total_monster_kill_count,
            totalClearCount = value.total_clear_count,
        };
    }
}