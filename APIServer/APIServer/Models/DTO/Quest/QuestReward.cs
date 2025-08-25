using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;


public class RewardQuestRequest : RequestBase
{
    [Required]
    public long questCode { get; set; }
}

public class RewardQuestResponse : ResponseBase
{
    
}