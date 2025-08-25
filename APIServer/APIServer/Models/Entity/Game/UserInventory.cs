using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 인벤토리 아이템 데이터 
/// 테이블 : user_inventory_item 
/// </summary>
public class UserInventoryItem
{
    public long item_id { get; set; }                // 아이템 ID
    public long item_code { get; set; }              // 아이템 식별 코드
    public int level { get; set; }                  // 아이템 레벨
}

/// <summary>
/// 유저 인벤토리 룬 데이터 
/// 테이블 : user_inventory_rune
/// </summary>
public class UserInventoryRune
{
    public long rune_id { get; set; }                // 룬 ID
    public long rune_code { get; set; }              // 룬 식별 코드
    public int level { get; set; }                  // 룬 레벨
}

/// <summary>
/// 유저 인벤토리 캐릭터 정보 테이블 데이터 
/// 테이블 : user_inventory_character
/// </summary>
public class UserInventoryCharacter
{
    public long character_id { get; set; }           // 캐릭터 ID
    public long character_code { get; set; }         // 캐릭터 식별 코드
    public int level { get; set; }                  // 캐릭터 레벨
}


