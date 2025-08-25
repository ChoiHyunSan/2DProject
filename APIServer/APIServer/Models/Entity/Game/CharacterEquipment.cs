using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 인벤토리의 캐릭터가 장착한 아이템 정보 테이블 데이터 
/// 테이블 : character_equipment_item
/// </summary>
public class CharacterEquipmentItem
{
    public long character_id { get; set; }       // 캐릭터 ID
    public long item_id { get; set; }            // 아이템 ID
}

/// <summary>
/// 유저 인벤토리의 캐릭터가 장착한 룬 정보
/// 테이블 : character_equipment_rune
/// </summary>
public class CharacterEquipmentRune
{
    public long character_id { get; set; }       // 캐릭터 ID
    public long rune_id { get; set; }            // 룬 ID
}