namespace APIServer.Models.DTO;

public class EquipmentItemRequest : RequestBase
{
    public long characterId { get; set; }
    public long itemId { get; set; }
}

public class EquipmentItemResponse : ResponseBase
{
    
}

public class EquipmentRuneRequest : RequestBase
{
    public long characterId { get; set; }
    public long runeId { get; set; }
}

public class EquipmentRuneResponse : ResponseBase
{
    
}