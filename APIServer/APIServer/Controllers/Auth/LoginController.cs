using APIServer.Models.DTO;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(ILogger<LoginController> logger, IAccountService accountService, IDataLoadService dataLoadService) 
    : ControllerBase
{
    private readonly ILogger<LoginController> _logger = logger;
    private readonly IAccountService _accountService = accountService;
    private readonly IDataLoadService _dataLoadService = dataLoadService;
    
    /// <summary>
    /// 게임 로그인 요청 API
    /// 세션 인증 : X
    /// 반환 값 :
    /// - 토큰 : 세션 인증이 필요한 API 요청시에 구분 값으로 사용하는 토큰으로, 만료 시간 존재
    /// - 반환 코드 : 로그인 요청 결과 (성공 : ErrorCode.None)
    /// - 게임 데이터 : 유저 데이터, 인벤토리, 퀘스트 정보 등 클라이언트에서 필요한 데이터
    /// </summary>
    [HttpPost]
    public async Task<LoginResponse> LoginAsync([FromBody] LoginRequest request)
    {
        LogInfo(_logger, EventType.Login, "Request Login", new { request });
        
        var login = await _accountService.LoginAsync(request.email, request.password);
        if (login.IsFailed)
        {
            return new LoginResponse { code = login.ErrorCode };
        }

        var dataLoad = await _dataLoadService.LoadGameDataAsync(login.Value.userId);
        if (dataLoad.IsFailed)
        {
            return new LoginResponse { code = dataLoad.ErrorCode };
        }
        
        return new LoginResponse
        {
            authToken = login.Value.authToken,
            gameData = dataLoad.Value
        };
    }
}
