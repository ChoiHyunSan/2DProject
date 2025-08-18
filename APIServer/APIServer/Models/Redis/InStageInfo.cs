using APIServer.Models.DTO;
using APIServer.Models.Entity.Data;

namespace APIServer.Models.Redis;

public class InStageInfo
{
    public long userId { get; set; }                                    // 유저 ID
    public string email { get; set; }                                   // 유저 이메일
    public long stageCode { get; set; }                                 // 스테이지 식별 코드
    public DateTime startTime { get; set; }                             // 스테이지 입장 시간
    public Dictionary<long , int> monsterKillTargets { get; set; }      // 몬스터 별 처치 목표 수 
    public Dictionary<long , int> monsterKills { get; set; }            // 몬스터 별 처치 마릿수

    public static InStageInfo Create(long userId, string email, long stageCode, List<StageMonsterInfo> monsterInfos)
    {
        return new InStageInfo()
        {
            userId = userId,
            email = email,
            stageCode = stageCode,
            startTime = DateTime.UtcNow,
            monsterKillTargets = monsterInfos.ToDictionary(x => x.monsterCode, x => x.monsterCount),
            monsterKills = monsterInfos.ToDictionary(x =>  x.monsterCode, x => 0)
        };
    }
}