namespace APIServer.Models.DTO.Inventory;

public class GetInventoryItemRequest : PageableBase
{
    
}

public class GetInventoryItemResponse : ResponseBase
{
    public List<ItemData> items { get; set; } = []; 
}