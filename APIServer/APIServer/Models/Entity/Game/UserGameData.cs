using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 게임 내 유저 정보 테이블 데이터 
/// 테이블 : user_game_data 
/// </summary>
public class UserGameData
{
    [Column("gold")]
    public int gold { get; set; }                       // 골드
    
    [Column("gem")]
    public int gem { get; set; }                        // 유료 재화
    
    [Column("exp")]
    public int exp { get; set; }                        // 경험치
    
    [Column("level")]
    public int level { get; set; }                      // 레벨
    
    [Column("total_monster_kill_count")]
    public int totalMonsterKillCount { get; set; }      // 총 몬스터 처치 횟수
    
    [Column("total_clear_count")]
    public int totalClearCount { get; set; }            // 총 스테이지 클리어 횟수
}