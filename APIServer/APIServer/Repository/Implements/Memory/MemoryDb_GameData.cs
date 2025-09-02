using APIServer.Models.DTO;
using APIServer.Models.Entity;
using CloudStructures.Structures;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(60);

    private RedisString<T> CreateRedisHandler<T>(string key) => new RedisString<T>(_conn, key, null);

    private async Task<bool> TrySetOrInvalidateAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        try
        {
            var ok = await CreateRedisHandler<T>(key).SetAsync(value, ttl ?? DefaultTtl);
            if (!ok)
                _ = await CreateRedisHandler<T>(key).DeleteAsync();
            return ok;
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "TrySetOrInvalidate failed", key, ex);
            _ = await CreateRedisHandler<T>(key).DeleteAsync();
            return false;
        }
    }

    private async Task<Result> TryDeleteAsync<T>(string key, string logMsg)
    {
        try
        {
            _ = await CreateRedisHandler<T>(key).DeleteAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.DeleteCacheData, logMsg, key, ex);
            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    private void LogCacheError(EventType evt, string msg, string key, Exception ex)
    {
        LogError(_logger, ErrorCode.FailedCacheGameData, evt, msg,
            new { key, ex.Message, ex.StackTrace });
    }

    // ================================
    // Cache Methods
    // ================================
    public async Task<bool> CacheUserGameData(long userId, UserGameData gameData)
    {
        var key = CreateUserGameDataKey(userId);
        return await TrySetOrInvalidateAsync(key, gameData, DefaultTtl);
    }

    public async Task<bool> CacheCharacterDataList(long userId, List<CharacterData> characterDataList)
    {
        var key = CreateCharacterDataKey(userId);
        return await TrySetOrInvalidateAsync(key, characterDataList, DefaultTtl);
    }

    public async Task<bool> CacheItemDataList(long userId, List<ItemData> itemDataList)
    {
        var key = CreateItemDataKey(userId);
        return await TrySetOrInvalidateAsync(key, itemDataList, DefaultTtl);
    }

    public async Task<bool> CacheRuneDataList(long userId, List<RuneData> runeDataList)
    {
        var key = CreateRuneDataKey(userId);
        return await TrySetOrInvalidateAsync(key, runeDataList, DefaultTtl);
    }

    public async Task<Result> CacheQuestList(long userId, List<UserQuestInprogress> progressList)
    {
        var key = CreateQuestKey(userId);
        var ok = await TrySetOrInvalidateAsync(key, progressList, DefaultTtl);
        return ok ? Result.Success() : Result.Failure(ErrorCode.FailedCacheGameData);
    }

    // ================================
    // GetCached Methods
    // ================================
    public async Task<Result<UserGameData>> GetCachedUserGameData(long userId)
    {
        var key = CreateUserGameDataKey(userId);
        try
        {
            var result = await CreateRedisHandler<UserGameData>(key).GetAsync();
            if (result.HasValue)
                return Result<UserGameData>.Success(result.Value);

            return Result<UserGameData>.Failure(ErrorCode.CannotFindUserGameData);
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "Failed Get Cached User Game Data", key, ex);
            return Result<UserGameData>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<UserQuestInprogress>>> GetCachedQuestList(long userId)
    {
        var key = CreateQuestKey(userId);
        try
        {
            var result = await CreateRedisHandler<List<UserQuestInprogress>>(key).GetAsync();
            if (result.HasValue)
                return Result<List<UserQuestInprogress>>.Success(result.Value);

            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.CannotFindQuestList);
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "Failed Get Cached Quest List", key, ex);
            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<CharacterData>>> GetCachedCharacterDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);
        try
        {
            var result = await CreateRedisHandler<List<CharacterData>>(key).GetAsync();
            if (result.HasValue)
                return Result<List<CharacterData>>.Success(result.Value);

            return Result<List<CharacterData>>.Failure(ErrorCode.CannotFindCharacterData);
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "Failed Get Character Data List", key, ex);
            return Result<List<CharacterData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<ItemData>>> GetCachedItemDataList(long userId)
    {
        var key = CreateItemDataKey(userId);
        try
        {
            var result = await CreateRedisHandler<List<ItemData>>(key).GetAsync();
            if (result.HasValue)
                return Result<List<ItemData>>.Success(result.Value);

            return Result<List<ItemData>>.Failure(ErrorCode.CannotFindItemData);
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "Failed Get Item Data List", key, ex);
            return Result<List<ItemData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<RuneData>>> GetCachedRuneDataList(long userId)
    {
        var key = CreateRuneDataKey(userId);
        try
        {
            var result = await CreateRedisHandler<List<RuneData>>(key).GetAsync();
            if (result.HasValue)
                return Result<List<RuneData>>.Success(result.Value);

            return Result<List<RuneData>>.Failure(ErrorCode.CannotFindRuneData);
        }
        catch (Exception ex)
        {
            LogCacheError(EventType.CacheGameData, "Failed Get Rune Data List", key, ex);
            return Result<List<RuneData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    // ================================
    // DeleteCached Methods
    // ================================
    public async Task<Result> DeleteCachedUserGameData(long userId)
    {
        var key = CreateUserGameDataKey(userId);
        return await TryDeleteAsync<UserGameData>(key, "Failed Delete Cached User Game Data");
    }

    public async Task<Result> DeleteCachedQuestList(long userId)
    {
        var key = CreateQuestKey(userId);
        return await TryDeleteAsync<List<UserQuestInprogress>>(key, "Failed Delete Cached Quest List");
    }

    public async Task<Result> DeleteCachedCharacterDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);
        return await TryDeleteAsync<List<CharacterData>>(key, "Failed Delete Cached Character Data List");
    }

    public async Task<Result> DeleteCachedItemDataList(long userId)
    {
        var key = CreateItemDataKey(userId);
        return await TryDeleteAsync<List<ItemData>>(key, "Failed Delete Cached Item Data List");
    }

    public async Task<Result> DeleteCachedRuneDataList(long userId)
    {
        var key = CreateRuneDataKey(userId);
        return await TryDeleteAsync<List<RuneData>>(key, "Failed Delete Cached Rune Data List");
    }

    public async Task<Result> DeleteCacheData(long userId, List<CacheType> cacheTypeList)
    {
        var tasks = cacheTypeList.Select(type => type switch
        {
            CacheType.Character    => DeleteCachedCharacterDataList(userId),
            CacheType.Item         => DeleteCachedItemDataList(userId),
            CacheType.Rune         => DeleteCachedRuneDataList(userId),
            CacheType.Quest        => DeleteCachedQuestList(userId),
            CacheType.UserGameData => DeleteCachedUserGameData(userId),
            _ => Task.FromResult(Result.Success())
        });

        var results = await Task.WhenAll(tasks);

        var failed = results.FirstOrDefault(r => r.IsFailed);
        return failed.IsFailed ? failed : Result.Success();
    }
}

public enum CacheType
{
    Character,
    Item,
    Rune,
    Quest,
    UserGameData,
}
