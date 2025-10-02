using APIServer.Models.DTO.Chat;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Chat;

[ApiController]
[Route("api/[controller]")] 
public class SendChatController(ILogger<SendChatController> logger, IChatService chatService)
    : ControllerBase
{
    private readonly ILogger<SendChatController> _logger = logger;
    private readonly IChatService _chatService = chatService;
    
    /// <summary>
    /// 채팅 입력 요청 API
    /// 세션 인증 : O
    /// 반환 값 : 채팅 입력 요청 결과
    /// </summary>
    [HttpPost]
    public async Task<SendChatResponse> SendChatAsync([FromBody] SendChatRequest request)
    {
        LogInfo(_logger, EventType.SendChat, "Request Send Chat", new { request.email });
        
        var result = await _chatService.SendAsync(request.email, request.message);
        if (result.IsFailed)
        {
            return new SendChatResponse {code = result.ErrorCode};      
        }
        
        return new SendChatResponse
        {
            messageId = result.Value.messageId,
            code = result.ErrorCode
        };       
    }
}