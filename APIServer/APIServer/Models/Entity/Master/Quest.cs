using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 퀘스트 정보 데이터
/// 테이블 : quest_info_data
/// </summary>
public class QuestInfoData
{
    [Column("quest_code")]
    public long questCode { get; set; }                         // 퀘스트 식별 코드
    
    [Column("name")]
    public string name { get; set; } = string.Empty;            // 퀘스트 이름
    
    [Column("description")]
    public string description { get; set; } = string.Empty;     // 퀘스트 설명
    
    [Column("reward_gold")]
    public int rewardGold { get; set; }                         // 보상 골드 획득량
    
    [Column("reward_gem")]
    public int rewardGem { get; set; }                          // 보상 잼 획득량
    
    [Column("reward_exp")]
    public int rewardExp { get; set; }                          // 보상 경험치 획득량
}