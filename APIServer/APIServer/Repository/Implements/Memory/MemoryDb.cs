using APIServer.Config;
using Microsoft.Extensions.Options;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb(IOptions<DbConfig> config, ILogger<MemoryDb> logger)
    : RedisBase(config.Value.Redis), IMemoryDb
{   
    // Logger
    private readonly ILogger<MemoryDb> _logger = logger;
}