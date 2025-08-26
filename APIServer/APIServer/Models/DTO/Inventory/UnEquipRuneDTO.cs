namespace APIServer.Models.DTO.Inventory;

public class UnEquipRuneRequest : RequestBase
{
    public long characterId { get; set; }
    public long runeId { get; set; }
}

public class UnEquipRuneResponse : ResponseBase
{
    
}