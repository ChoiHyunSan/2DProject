using APIServer.Models.Entity.Data;

namespace APIServer.Repository;

public interface IMasterDb
{
    /// <summary>
    /// Master Db 데이터를 메모리로 로드하는 메서드
    ///
    /// 반환 값 : 에러 코드 (성공 : ErrorCode.None)
    /// </summary>
    public Task<ErrorCode> Load();

    /// <summary>
    /// 
    /// 
    /// </summary>
    Task<(ErrorCode, CharacterOriginData)> GetCharacterOriginDataAsync(long characterCode);
}