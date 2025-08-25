using System.ComponentModel.DataAnnotations.Schema;

namespace APIServer.Models.Entity;

/// <summary>
/// 게임 내 유저 정보 테이블 데이터 
/// 테이블 : user_game_data 
/// </summary>
public class UserGameData
{
    public long user_id { get; set; }                    // 유저 ID
    public int gold { get; set; }                       // 골드
    public int gem { get; set; }                        // 유료 재화
    public int exp { get; set; }                        // 경험치
    public int level { get; set; }                      // 레벨
    public int total_monster_kill_count { get; set; }      // 총 몬스터 처치 횟수
    public int total_clear_count { get; set; }            // 총 스테이지 클리어 횟수
    
    public override string ToString()
    {
        return $"[User ID: {user_id}, Gold: {gold}, Gem: {gem}, Exp: {exp}, Level: {level}, Total Monster Kill Count: {total_monster_kill_count}, Total Clear Count: {total_clear_count}]";
    }
}