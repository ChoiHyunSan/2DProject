using APIServer.Models.DTO;
using APIServer.Repository;

namespace APIServer.Service.Implements;

public class TestService(ILogger<TestService> logger, IGameDb gameDb, IAccountDb accountDb)
    : ITestService
{
    private readonly ILogger<TestService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IAccountDb _accountDb = accountDb;

    public async Task<TestResponse> TestAsync(TestRequest request)
    {
        _logger.LogInformation("Request : {RequestName}, {RequestPassword}", request.name, request.password);

        var testUser = await _gameDb.TestInsert();

        return new TestResponse
        {
            message = testUser.ToString()
        };
    }
}