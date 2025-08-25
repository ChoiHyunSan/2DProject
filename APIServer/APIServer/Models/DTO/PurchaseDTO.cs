using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class PurchaseCharacterRequest : RequestBase
{
    [Required]
    public long characterCode { get; set; }
}

public class PurchaseCharacterResponse : ResponseBase
{
    public long characterCode { get; set; }
    public int currentGold { get; set; }
    public int currentGem { get; set; }
}

