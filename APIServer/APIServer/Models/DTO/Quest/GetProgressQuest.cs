namespace APIServer.Models.DTO.Quest;

public class GetProgressQuestRequest : PageableBase
{
    
}

public class GetProgressQuestResponse : ResponseBase
{
    public List<ProgressQuest> progressQuests { get; set; } = [];  
}

public class ProgressQuest
{
    public long questCode { get; set; }     // 퀘스트 식별 코드
    public int progress { get; set; }       // 퀘스트 진행도
}