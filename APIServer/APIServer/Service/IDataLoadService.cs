using APIServer.Models.DTO;
using APIServer.Models.DTO.Quest;
using APIServer.Models.Entity;

namespace APIServer.Service;

public interface IDataLoadService
{
    /// <summary> 전체 게임 데이터 조회 </summary>
    Task<Result<FullGameData>> LoadGameDataAsync(long userId);
    
    /// <summary> 진행중인 퀘스트 목록 페이징조회 </summary>
    Task<Result<List<UserQuestInprogress>>> GetProgressQuestListAsync(long userId, Pageable pageable);
    
    /// <summary> 완료된 퀘스트 목록 페이징 조회 </summary>
    Task<Result<List<UserQuestComplete>>> GetCompleteQuestListAsync(long userId, Pageable pageable);
    
    /// <summary> 인벤토리 캐릭터 목록 조회 </summary>
    Task<Result<List<CharacterData>>> GetInventoryCharacterListAsync(long userId);
    
    /// <summary> 인벤토리 아이템 목록 페이징 조회 </summary>
    Task<Result<List<ItemData>>> GetInventoryItemListAsync(long userId, Pageable requestPageable);
   
    /// <summary> 인벤토리 룬 목록 페이징 조회 </summary>
    Task<Result<List<RuneData>>> GetInventoryRuneListAsync(long userId, Pageable requestPageable);
    
    /// <summary> 유저 게임 데이터 조회 </summary>
    Task<Result<UserGameData>> GetUserGameDataAsync(long userId);
}