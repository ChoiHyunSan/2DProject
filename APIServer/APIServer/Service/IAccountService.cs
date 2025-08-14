using APIServer.Models.DTO;

namespace APIServer.Service;

public interface IAccountService
{
    /// <summary>
    /// 회원가입 비즈니스 로직 메서드
    /// 1) 이메일 중복 확인
    /// 2) 게임 기본 데이터 생성
    /// 3) 계정 정보 생성
    /// - 생성 과정에서 오류 발생 시, 롤백한다.
    /// 
    /// 반환 값 : 요청 결과 (ErrorCode.None인 경우, 성공)
    /// </summary>
    Task<ErrorCode> RegisterAccountAsync(string email, string password);
    
    
    /// <summary>
    /// 로그인 비즈니스 로직 메서드
    /// 1) 이메일 & 비밀번호 검증
    /// 2) 인증 토큰 발급
    /// 3) 인증 세션 생성 
    /// 4) 게임 데이터 로드
    /// 5) 게임 데이터 캐싱
    /// 6) 결과 값 반환
    /// 
    /// 반환 값 : (게임 데이터, 인증 토큰, 요청 결과)
    /// </summary>
    Task<(GameData?, string, ErrorCode)> LoginAsync(string email, string password);
}