using System.ComponentModel.DataAnnotations;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Redis;

namespace APIServer.Repository;

public interface IMemoryDb
{
    /// <summary> 세션 저장 </summary>
    Task<bool> RegisterSessionAsync(UserSession session);
    
    /// <summary> 이메일 기반 세션 조회 </summary>
    Task<Result<UserSession>> GetSessionByEmail(string email);

    /// <summary> 세션 Lock </summary>
    Task<Result> TrySessionRequestLock(long userId, TimeSpan? ttl = null);
    
    /// <summary> 세션 UnLock </summary>
    Task<Result> TrySessionRequestUnLock(long userId);

    /// <summary> 스테이지 인게임 정보 캐싱 </summary>
    Task<bool> CacheStageInfo(InStageInfo inStageInfo);

    /// <summary> 스테이지 인게임 정보 조회 </summary>
    Task<Result<InStageInfo>> GetGameInfo(long userId);

    /// <summary> 스테이지 인게임 정보 삭제 </summary>
    Task<bool> DeleteStageInfo(InStageInfo stageInfo);
    
    /// <summary> 퀘스트 리스트 캐시정보 조회 </summary>
    Task<Result<List<UserQuestInprogress>>> GetCachedQuestList(long userId);
    
    /// <summary> 퀘스트 리스트 캐싱 </summary>
    Task<Result> CacheQuestList(long userId, List<UserQuestInprogress> progressList);
    
    /// <summary> 퀘스트 리스트 캐시 삭제 </summary>
    Task<Result> DeleteCachedQuestList(long userId);
    
    /// <summary> 게임 데이터 캐싱 </summary>
    Task<Result<UserGameData>> GetCachedUserGameData(long userId);
    
    /// <summary> 캐릭터 리스트 캐시정보 조회 </summary>
    Task<Result<List<CharacterData>>> GetCachedCharacterDataList(long userId);
    
    /// <summary> 아이템 리스트 캐시정보 조회 </summary>
    Task<Result<List<ItemData>>> GetCachedItemDataList(long userId);
    
    /// <summary> 룬 리스트 캐시정보 조회 </summary>
    Task<Result<List<RuneData>>> GetCachedRuneDataList(long userId);
    
    /// <summary> 유저 데이터 캐싱 </summary>
    Task<bool> CacheUserGameData(long userId, UserGameData userData);
    
    /// <summary> 캐릭터 리스트 캐싱 </summary>
    Task<bool> CacheCharacterDataList(long userId, List<CharacterData> characterDataList);
    
    /// <summary> 아이템 리스트 캐싱 </summary>
    Task<bool> CacheItemDataList(long userId, List<ItemData> itemDataList);
    
    /// <summary> 룬 리스트 캐싱 </summary>
    Task<bool> CacheRuneDataList(long userId, List<RuneData> runeDataList);
    
    /// <summary> 유저 데이터 캐시 삭제 </summary>
    Task<Result> DeleteCachedUserGameData(long userId);
    
    /// <summary> 캐릭터 리스트 캐시 삭제 </summary>
    Task<Result> DeleteCachedCharacterDataList(long userId);
    
    /// <summary> 아이템 리스트 캐시 삭제 </summary>
    Task<Result> DeleteCachedItemDataList(long userId);
    
    /// <summary> 룬 리스트 캐시 삭제 </summary>
    Task<Result> DeleteCachedRuneDataList(long userId);
}