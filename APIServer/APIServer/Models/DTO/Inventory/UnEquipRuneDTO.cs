using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO.Inventory;

public class UnEquipRuneRequest : RequestBase
{
    [Required]
    public long characterId { get; set; }
    [Required]
    public long runeId { get; set; }
}

public class UnEquipRuneResponse : ResponseBase
{
    
}