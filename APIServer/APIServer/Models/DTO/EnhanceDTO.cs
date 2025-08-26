using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class EnhanceItemRequest : RequestBase
{
    [Required]
    public long itemId { get; set; }
}

public class EnhanceItemResponse : ResponseBase
{
    
}

public class EnhanceRuneRequest : RequestBase
{
    [Required]
    public long runeId { get; set; }
}

public class EnhanceRuneResponse : ResponseBase
{
    
}

public class EnhanceCharacterRequest : RequestBase
{
    public long characterId { get; set; }   
}

public class EnhanceCharacterResponse : ResponseBase
{
    
}