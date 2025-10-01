using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO.Inventory;

public class UnEquipItemRequest : RequestBase
{
    [Required]
    public long characterId { get; set; }
    [Required]
    public long itemId { get; set; }
}

public class UnEquipItemResponse : ResponseBase
{
    
}