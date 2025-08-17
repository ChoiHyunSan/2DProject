using APIServer.Models.DTO;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegisterAccountController(ILogger<RegisterAccountController> logger, IAccountService accountService) : ControllerBase
{
    private readonly ILogger<RegisterAccountController> _logger = logger;
    private readonly IAccountService _accountService = accountService;

    /// <summary>
    /// 게임 계정 회원가입 요청 API
    /// 세션 인증 : X
    /// 반환 값 : 회원가입 요청 결과
    /// </summary>
    [HttpPost]
    public async Task<RegisterAccountResponse> RegisterAccountAsync([FromBody] RegisterAccountRequest request)
    {
        LogInfo(_logger, EventType.CreateAccount , $"Request Register Account", new { request });
        
        var errorCode = await accountService.RegisterAccountAsync(request.email, request.password);
        return new RegisterAccountResponse { code = errorCode };
    }
}