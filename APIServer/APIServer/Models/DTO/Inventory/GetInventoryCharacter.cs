namespace APIServer.Models.DTO.Inventory;

public class GetInventoryCharacterRequest : RequestBase
{
    
}

public class GetInventoryCharacterResponse : ResponseBase
{
    public List<CharacterData> characters { get; set; } = [];
}