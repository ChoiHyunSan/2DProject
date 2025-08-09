using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 아이템 원본 데이터
/// 테이블 : item_origin_data
/// </summary>
public class ItemOriginData
{
    [Column("item_code")]
    public long itemCode { get; set; }                          // 아이템 식별 코드
    
    [Column("name")]
    public string name { get; set; } = string.Empty;            // 아이템 이름
    
    [Column("description")]
    public string description { get; set; } = string.Empty;     // 아이템 설명
}

/// <summary>
/// 아이템 레벨 별 강화 정보
/// 테이블 : item_enhance_data
/// </summary>
public class ItemEnhanceData
{
    [Column("item_code")]
    public long itemCode { get; set; }                          // 아이템 식별 코드
    
    [Column("level")]
    public int level { get; set; }                              // 아이템 레벨
    
    [Column("attackDamage")]
    public int attackDamage { get; set; }                       // 추가 공격력
    
    [Column("defense")]
    public int defense { get; set; }                            // 추가 방어력
    
    [Column("max_hp")]
    public int maxHp { get; set; }                              // 추가 체력
    
    [Column("critical_chance")]
    public int criticalChance { get; set; }                     // 추가 치명타 확률
    
    [Column("enhance_price")]
    public int enhancePrice { get; set; }                       // 강화 가격
    
    [Column("sell_price")]
    public int sellPrice { get; set; }                          // 판매 가격
}