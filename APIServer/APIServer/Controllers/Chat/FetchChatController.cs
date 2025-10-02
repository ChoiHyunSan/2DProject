using APIServer.Models.DTO.Chat;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;
using static APIServer.LoggerManager;

namespace APIServer.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class FetchChatController(ILogger<FetchChatController> logger, IChatService chatService)
    : ControllerBase
{
    private readonly ILogger<FetchChatController> _logger = logger;
    private readonly IChatService _chatService = chatService;
    
    /// <summary>
    /// 채팅 패치 요청 API
    /// 세션 인증 : O
    /// 반환 값 : 갱신된 메시지 리스트
    /// </summary>
    [HttpPost]
    public async Task<FetchChatResponse> FetchChatAsync([FromBody] FetchChatRequest request)
    {
        LogInfo(_logger, EventType.FetchChat, "Request Fetch Chat", new { request.email });

        var result = await _chatService.FetchAsync(request.limit, request.latest);
        if (result.IsFailed)
        {
            return new FetchChatResponse { code = result.ErrorCode };
        }
        
        return new FetchChatResponse
        {
            messages = result.Value,
            code = result.ErrorCode
        };       
    }
}