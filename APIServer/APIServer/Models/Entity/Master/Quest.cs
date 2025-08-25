using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 퀘스트 정보 데이터
/// 테이블 : quest_info_data
/// </summary>
public class QuestInfoData
{
    [Column("quest_code")]
    public long quest_code { get; set; }                         // 퀘스트 식별 코드
    
    [Column("name")]
    public string name { get; set; } = string.Empty;            // 퀘스트 이름
    
    [Column("description")]
    public string description { get; set; } = string.Empty;     // 퀘스트 설명
    
    [Column("quest_type")]
    public QuestType quest_type { get; set; }                    // 퀘스트 타입 

    [Column("quest_progress")]
    public int quest_progress { get; set; }                      // 퀘스트 클리어 값
    
    [Column("reward_gold")]
    public int reward_gold { get; set; }                         // 보상 골드 획득량
    
    [Column("reward_gem")]
    public int reward_gem { get; set; }                          // 보상 잼 획득량
    
    [Column("reward_exp")]
    public int reward_exp { get; set; }                          // 보상 경험치 획득량
}

public enum QuestType
{
    GetGold = 1,
    GetExp = 2,
    ClearStage = 3,
    KillMonster = 4,
    GetItem = 5,
}
