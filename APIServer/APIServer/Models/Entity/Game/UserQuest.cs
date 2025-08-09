using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 퀘스트 진행 데이터
/// 테이블 : user_quest_inprogress
/// </summary>
public class UserQuestInprogress
{
    [Column("quest_inprogress_id")]
    public long id { get; set; }                // ID
    
    [Column("quest_code")]
    public long questCode { get; set; }         // 퀘스트 식별 코드
    
    [Column("progress")]
    public int progress { get; set; }           // 퀘스트 진행 사항 (ex. 골드 10000 획득 -> 획득한 골드 정보 표시)
    
    [Column("expire_date")]
    public DateTime expireDate { get; set; }    // 퀘스트 만료 시간
}

/// <summary>
/// 유저 퀘스트 완료 데이터
/// 테이블 : user_quest_complete
/// </summary>
public class UserQuestComplete
{
    [Column("quest_complete_id")]
    public long id { get; set; }                // ID
    
    [Column("quest_code")]
    public long questCode { get; set; }         // 퀘스트 식별 코드
    
    [Column("complete_date")]
    public DateTime completeDate { get; set; }  // 퀘스트 완료 시간
}


