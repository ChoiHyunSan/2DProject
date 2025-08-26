namespace APIServer.Models.DTO.Inventory;

public class UnEquipItemRequest : RequestBase
{
    public long characterId { get; set; }
    public long itemId { get; set; }
}

public class UnEquipItemResponse : ResponseBase
{
    
}