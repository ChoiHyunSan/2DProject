namespace APIServer.Models.DTO.Inventory;

public class GetInventoryRuneRequest : PageableBase
{
    
}

public class GetInventoryRuneResponse : ResponseBase
{
    public List<RuneData> runes { get; set; } = [];
}