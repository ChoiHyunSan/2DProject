using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbTestController(ILogger<DbTestController> logger, ITestService testService) : ControllerBase
{
    private readonly ILogger<DbTestController> _logger = logger;
    private readonly ITestService _testService = testService;

    [HttpPost]
    public async Task<TestResponse> TestAsync([FromBody] TestRequest request)
    {
        var response = new TestResponse();
        
        var session = (UserSession)HttpContext.Items["userSession"];
        if (session == null)
        {
            _logger.LogInformation("Session is null");
            return response;       
        }
        
        _logger.LogInformation("TestAsync");
        
        response = await _testService.TestAsync(request);
        return response;
    }
}