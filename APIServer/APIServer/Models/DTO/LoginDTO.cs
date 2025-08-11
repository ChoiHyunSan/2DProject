using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public record LoginRequest
{
    [Required(ErrorMessage = "email is required")]
    [EmailAddress(ErrorMessage = "email is invalid")]
    public string email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "password is required")]
    [MinLength(8, ErrorMessage = "password`s length must be greater than 8")]
    public string password { get; set; } = string.Empty;
}

public class LoginResponse : ResponseBase
{
    public string authToken { get; set; } = string.Empty;
    public GameData? gameData { get; set; } = new();
}

public class GameData
{
    public int gold { get; set; }
    public int gem { get; set; }
    public int exp { get; set; }
    public int level { get; set; }
    public int totalMonsterKillCount { get; set; }
    public int totalClearCount { get; set; }
    
    public List<CharacterData> characters { get; set; } = [];
    public List<ItemData> items { get; set; } = [];
    public List<RuneData> runes { get; set; } = [];
    public List<QuestData> quests { get; set; } = [];
    public List<ClearStageData> clearStages { get; set; } = [];
}

public class CharacterData
{
    public long characterCode { get; set; }
    public int level { get; set; }
    public List<ItemData> equipItems { get; set; } = [];
    public List<RuneData> equipRunes { get; set; } = [];
}

public class ItemData
{
    public long itemCode { get; set; }
    public int level { get; set; }
}

public class RuneData
{
    public long runeCode { get; set; }
    public int level { get; set; }
}

public class QuestData
{
    public long questCode { get; set; }
    public int progress { get; set; }
    public DateTime expireDate { get; set; }
}

public class ClearStageData
{
    public long stageCode { get; set; }
}