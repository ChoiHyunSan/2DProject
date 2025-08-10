using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 스테이지 별 보상 아이템 데이터
/// 테이블 : stage_reward_item
/// </summary>
public class StageRewardItem
{
    [Column("stage_code")]
    public long stageCode { get; set; }     // 스테이지 식별 코드
    
    [Column("item_code")]
    public int itemCode { get; set; }       // 아이템 식별 코드
    
    [Column("level")]
    public int level { get; set; }          // 아이템 레벨
    
    [Column("drop_rate")]
    public int dropRate { get; set; }       // 드랍 확률
}

/// <summary>
/// 스테이지 별 보상 룬 데이터
/// 테이블 : stage_reward_rune
/// </summary>
public class StageRewardRune
{
    [Column("stage_code")]
    public long stageCode { get; set; }     // 스테이지 식별 코드
    
    [Column("rune_code")]
    public int runeCode { get; set; }       // 룬 식별 코드
    
    [Column("drop_rate")]
    public int dropRate { get; set; }       // 드랍 확률
}

/// <summary>
///  스테이지 별 보상 골드 데이터
///  테이블: stage_reward_gold
/// </summary>
public class StageRewardGold
{
    [Column("stage_code")]
    public long stageCode { get; set; }     // 스테이지 식별 코드
    
    [Column("gold")]
    public int gold { get; set; }           // 골드 획득량
}

/// <summary>
/// 스테이지 별 몬스터 출현 정보
/// 테이블 : stage_monster_info
/// </summary>
public class StageMonsterInfo
{
    public long stageCode { get; set; }      // 식별 코드
    public long monsterCode { get; set; }    // 몬스터 식별 코드
    public int monsterCount { get; set; }    // 몬스터 등장 마릿수
}