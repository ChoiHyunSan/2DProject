using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 아이템 원본 데이터
/// 테이블 : item_origin_data
/// </summary>
public class ItemOriginData
{
    public long item_code { get; set; }                          // 아이템 식별 코드
    public string name { get; set; } = string.Empty;            // 아이템 이름
    public string description { get; set; } = string.Empty;     // 아이템 설명
}

/// <summary>
/// 아이템 레벨 별 강화 정보
/// 테이블 : item_enhance_data
/// </summary>
public class ItemEnhanceData
{
    public long item_code { get; set; }                          // 아이템 식별 코드
    public int level { get; set; }                              // 아이템 레벨
    public int attack_damage { get; set; }                       // 추가 공격력
    public int defense { get; set; }                            // 추가 방어력
    public int maxHp { get; set; }                              // 추가 체력
    public int critical_chance { get; set; }                     // 추가 치명타 확률
    public int enhance_price { get; set; }                       // 강화 가격
    public int sell_price { get; set; }                          // 판매 가격
}