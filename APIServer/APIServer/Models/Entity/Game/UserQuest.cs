using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 유저 퀘스트 진행 데이터
/// 테이블 : user_quest_inprogress
/// </summary>
public class UserQuestInprogress
{
    public long quest_inprogress_id { get; set; }                                // ID
    public long quest_code { get; set; }                         // 퀘스트 식별 코드
    public int progress { get; set; }                            // 퀘스트 진행 사항 (ex. 골드 10000 획득 -> 획득한 골드 정보 표시)
    public DateTime expire_date { get; set; }                    // 퀘스트 만료 시간
}

/// <summary>
/// 유저 퀘스트 완료 데이터
/// 테이블 : user_quest_complete
/// </summary>
public class UserQuestComplete
{
    public long quest_complete_id { get; set; }                // ID
    public long quest_code { get; set; }         // 퀘스트 식별 코드
    public DateTime complete_date { get; set; }  // 퀘스트 완료 시간
    public bool earn_reward { get; set; }        // 보상획득 여부
}


