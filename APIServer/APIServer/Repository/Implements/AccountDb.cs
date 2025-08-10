using APIServer.Config;
using Microsoft.Extensions.Options;

namespace APIServer.Repository.Implements;

public class AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> dbConfig)
    : MySQLBase(dbConfig.Value.AccountDb), IAccountDb
{
    private readonly ILogger<AccountDb> _logger = logger;
    
}