using APIServer.Models.DTO;

namespace APIServer.Service;

public interface ITestService
{
    public Task<TestResponse> TestAsync(TestRequest request);
}