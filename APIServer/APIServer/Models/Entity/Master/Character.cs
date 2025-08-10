using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 캐릭터 원본 데이터
/// 테이블 : character_origin_data
/// </summary>
public class CharacterOriginData
{
    [Column("character_code")]
    public long characterCode { get; set; }                     // 캐릭터 식별 코드
    
    [Column("name")]
    public string name { get; set; } = string.Empty;            // 캐릭터 이름
    
    [Column("description")]
    public string description { get; set; } = string.Empty;     // 캐릭터 설명
}

/// <summary>
/// 캐릭터 레벨 별 강화 가격
/// 테이블 : character_enhance_data
/// </summary>
public class CharacterEnhancePriceData
{
    [Column("character_code")]
    public long characterCode { get; set; }                     // 캐릭터 식별 코드
    
    [Column("level")]
    public int level { get; set; }                              // 캐릭터 레벨
    
    [Column("attack_damage")]
    public int attackDamage { get; set; }                       // 추가 공격력
    
    [Column("defense")]
    public int defense { get; set; }                            // 추가 방어력
    
    [Column("max_hp")]
    public int maxHp { get; set; }                              // 추가 최대 체력
    
    [Column("critical_chance")]
    public int criticalChance { get; set; }                     // 추가 치명타 확률
    
    [Column("enhance_price")]
    public int enhancePrice { get; set; }                       // 강화 가격
}