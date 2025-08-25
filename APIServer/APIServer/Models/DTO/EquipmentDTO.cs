using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class EquipmentItemRequest : RequestBase
{
    [Required]
    public long characterId { get; set; }
    [Required]
    public long itemId { get; set; }
}

public class EquipmentItemResponse : ResponseBase
{
    
}

public class EquipmentRuneRequest : RequestBase
{
    [Required]
    public long characterId { get; set; }
    [Required]
    public long runeId { get; set; }
}

public class EquipmentRuneResponse : ResponseBase
{
    
}