using APIServer.Models.DTO;

namespace APIServer.Service;

public interface IStageService
{
    /// <summary> 클리어 스테이지 목록 조회 </summary>
    Task<Result<List<StageInfo>>> GetClearStage(long userId);
    
    /// <summary> 스테이지 입장 </summary>
    Task<Result<List<MonsterInfo>>> EnterStage(long userId, string email, long stageCode, List<long> characterIds);
    
    /// <summary> 스테이지 내 몬스터 처치 </summary>
    Task<Result> KillMonster(string email, long monsterCode);
    
    /// <summary> 스테이지 완료 </summary>
    Task<Result> ClearStage(string email, long stageCode);
}