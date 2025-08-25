using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 스테이지 별 보상 아이템 데이터
/// 테이블 : stage_reward_item
/// </summary>
public class StageRewardItem
{
    public long stage_code { get; set; }     // 스테이지 식별 코드
    public int item_code { get; set; }       // 아이템 식별 코드
    public int level { get; set; }          // 아이템 레벨
    public int drop_rate { get; set; }       // 드랍 확률
}

/// <summary>
/// 스테이지 별 보상 룬 데이터
/// 테이블 : stage_reward_rune
/// </summary>
public class StageRewardRune
{
    public long stage_code { get; set; }     // 스테이지 식별 코드
    public int rune_code { get; set; }       // 룬 식별 코드
    public int drop_rate { get; set; }       // 드랍 확률
}

/// <summary>
///  스테이지 별 보상 골드 데이터
///  테이블: stage_reward_gold
/// </summary>
public class StageRewardGold
{
    public long stage_code { get; set; }     // 스테이지 식별 코드
    public int gold { get; set; }           // 골드 획득량
}

/// <summary>
/// 스테이지 별 몬스터 출현 정보
/// 테이블 : stage_monster_info
/// </summary>
public class StageMonsterInfo
{
    public long stage_code { get; set; }      // 식별 코드
    public long monster_code { get; set; }    // 몬스터 식별 코드
    public int monster_count { get; set; }    // 몬스터 등장 마릿수
}