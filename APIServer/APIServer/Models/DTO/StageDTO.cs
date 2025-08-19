using APIServer.Models.Entity;

namespace APIServer.Models.DTO;

public class GetClearStageRequest : RequestBase
{
    
}

public class GetClearStageResponse : ResponseBase
{
    public List<StageInfo> stageList { get; set; } = [];
}

public record StageInfo
{
    public long stageCode { get; set; }
    public int clearCount { get; set; }
    public DateTime lastClearDate { get; set; }

    public static StageInfo Of(UserClearStage userClearStage)
    {
        return new StageInfo
        {
            stageCode = userClearStage.stageCode,
            clearCount = userClearStage.clearCount,
            lastClearDate = userClearStage.lastClearDate
        };
    }
}

public class EnterStageRequest : RequestBase
{
    public long stageCode { get; set; }
    public List<long> characterIds { get; set; } = [];
}

public class EnterStageResponse : ResponseBase
{
    public List<MonsterInfo> monsterList { get; set; } = [];
}

public record MonsterInfo
{
    public long monsterCode { get; set; }
    public int monsterCount { get; set; }
}

public class KillMonsterRequest : RequestBase
{
    public long monsterCode { get; set; }
}

public class KillMonsterResponse : ResponseBase
{
    
}

public class StageClearRequest : RequestBase
{
    public long stageCode { get; set; }
}

public class StageClearResponse : ResponseBase
{
    
}