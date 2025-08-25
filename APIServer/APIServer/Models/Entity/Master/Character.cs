using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity.Data;

/// <summary>
/// 캐릭터 원본 데이터
/// 테이블 : character_origin_data
/// </summary>
public class CharacterOriginData
{
    public long character_code { get; set; }                     // 캐릭터 식별 코드
    public string name { get; set; } = string.Empty;            // 캐릭터 이름
    public string description { get; set; } = string.Empty;     // 캐릭터 설명
    public int price_gold { get; set; }                          // 구매 필요 골드 재화
    public int price_gem { get; set; }                           // 구매 필요 유료 재화
}

/// <summary>
/// 캐릭터 레벨 별 강화 가격
/// 테이블 : character_enhance_data
/// </summary>
public class CharacterEnhanceData
{
    public long character_code { get; set; }                     // 캐릭터 식별 코드
    public int level { get; set; }                               // 캐릭터 레벨
    public int attack_damage { get; set; }                       // 추가 공격력
    public int defense { get; set; }                             // 추가 방어력
    public int maxHp { get; set; }                               // 추가 최대 체력
    public int critical_chance { get; set; }                     // 추가 치명타 확률
    public int enhance_price { get; set; }                       // 강화 가격
}