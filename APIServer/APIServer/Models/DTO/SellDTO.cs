using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class ItemSellRequest : RequestBase
{
    [Required]
    public long itemId { get; set; }
}

public class ItemSellResponse : ResponseBase
{
    
}

public  class RuneSellRequest : RequestBase
{
    [Required]
    public long runeId { get; set; }
}

public class RuneSellResponse : ResponseBase
{
    
}