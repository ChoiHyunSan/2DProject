using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 룬 원본 데이터
/// 테이블 : rune_origin_data
/// </summary>
public class RuneOriginData
{
    [Column("rune_code")]
    public long runeCode { get; set; }                          // 룬 식별 코드
    
    [Column("name")]
    public string name { get; set; } = string.Empty;            // 룬 이름
    
    [Column("description")]
    public string description { get; set; } = string.Empty;     // 룬 설명
}

/// <summary>
/// 룬 레벨 별 강화 정보
/// 테이블 : rune_enhance_data
/// </summary>
public class RuneEnhanceData
{   
    [Column("rune_code")]
    public long runeCode { get; set; }                          // 룬 식별 코드
    
    [Column("level")]
    public int level { get; set; }                             // 룬 레벨
    
    [Column("attack_damage")]
    public int attackDamage { get; set; }                       // 추가 공격력
    
    [Column("defense")]
    public int defense { get; set; }                            // 추가 방어력
    
    [Column("max_hp")]
    public int maxHp { get; set; }                              // 추가 체력
    
    [Column("critical_chance")]
    public int criticalChance { get; set; }                     // 추가 치명타 확률
    
    [Column("enhance_price")]
    public int enhancePrice { get; set; }                       // 강화 필요 개수
}