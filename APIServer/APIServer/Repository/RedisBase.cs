using CloudStructures;

namespace APIServer.Repository;

public abstract class RedisBase
{
    protected RedisConnection _conn;

    protected RedisBase(string redisConfig)
    {
        var config = new RedisConfig("default", redisConfig);
        _conn = new RedisConnection(config);
    }
}