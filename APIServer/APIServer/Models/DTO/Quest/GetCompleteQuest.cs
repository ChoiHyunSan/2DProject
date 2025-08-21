namespace APIServer.Models.DTO.Quest;

public class GetCompleteQuestRequest : PageableBase
{
    
}

public class GetCompleteQuestResponse : ResponseBase
{
    public List<CompleteQuest> completeQuests { get; set; } = []; 
}

public class CompleteQuest
{
    public long questCode { get; set; }   
}