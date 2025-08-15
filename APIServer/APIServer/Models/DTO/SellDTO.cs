namespace APIServer.Models.DTO;

public class ItemSellRequest : RequestBase
{
    public long itemId { get; set; }
}

public class ItemSellResponse : ResponseBase
{
    
}

public  class RuneSellRequest : RequestBase
{
    public long runeId { get; set; }
}

public class RuneSellResponse : ResponseBase
{
    
}