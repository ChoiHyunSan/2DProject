using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 인벤토리 아이템 데이터 
/// 테이블 : user_inventory_item 
/// </summary>
public class UserInventoryItem
{
    [Column("item_id")]
    public long itemId { get; set; }                // 아이템 ID
    
    [Column("item_code")]
    public long itemCode { get; set; }              // 아이템 식별 코드
    
    [Column("level")]
    public int level { get; set; }                  // 아이템 레벨
}

/// <summary>
/// 유저 인벤토리 룬 데이터 
/// 테이블 : user_inventory_rune
/// </summary>
public class UserInventoryRune
{
    [Column("rune_id")]
    public long runeId { get; set; }                // 룬 ID
    
    [Column("rune_code")]
    public long runeCode { get; set; }              // 룬 식별 코드
    
    [Column("level")]
    public int level { get; set; }                  // 룬 레벨
}

/// <summary>
/// 유저 인벤토리 캐릭터 정보 테이블 데이터 
/// 테이블 : user_inventory_character
/// </summary>
public class UserInventoryCharacter
{
    [Column("character_id")]
    public long characterId { get; set; }           // 캐릭터 ID
    
    [Column("character_code")]
    public long characterCode { get; set; }         // 캐릭터 식별 코드
    
    [Column("level")]
    public int level { get; set; }                  // 캐릭터 레벨
}


