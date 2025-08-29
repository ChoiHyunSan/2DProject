using APIServer.Models.DTO.Mail;
using APIServer.Models.Entity;
using APIServer.Service.Implements;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Mail;

[ApiController]
[Route("api/[controller]")]   
public class GetMailController(ILogger<GetMailController> logger, IMailService mailService)
    : ControllerBase
{
    private readonly ILogger<GetMailController> _logger = logger;
    private readonly IMailService _mailService = mailService;

    [HttpPost]
    public async Task<GetMailResponse> GetMailAsync([FromBody] GetMailRequest request)
    {
        var session = HttpContext.Items["userSession"] as UserSession;
        
        LogInfo(_logger, EventType.GetMail, "Request Get Mail", new { session.userId });
        
        var result = await _mailService.GetMailAsync(session.userId, request.Pageable);
        if (result.IsFailed)
        {
            return new GetMailResponse { code = result.ErrorCode };       
        }
        
        return new GetMailResponse { code = result.ErrorCode, mails = result.Value };       
    }
}