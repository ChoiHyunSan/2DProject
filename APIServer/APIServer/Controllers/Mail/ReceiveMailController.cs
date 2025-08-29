using APIServer.Models.DTO.Mail;
using APIServer.Models.Entity;
using APIServer.Service.Implements;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Mail;

[ApiController]
[Route("api/[controller]")]  
public class ReceiveMailController(ILogger<ReceiveMailController> logger, IMailService mailService)
    : ControllerBase
{
    private readonly ILogger<ReceiveMailController> _logger = logger;
    private readonly IMailService _mailService = mailService;

    [HttpPost]
    public async Task<ReceiveMailResponse> ReceiveMailAsync([FromBody] ReceiveMailRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.ReceiveMail, "Request Receive Mail", new { session.userId , request.mailId });

        var result = await _mailService.ReceiveMailAsync(session.userId, request.mailId);
        return new ReceiveMailResponse { code = result.ErrorCode };       
    }
}