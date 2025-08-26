using APIServer.Models.DTO;
using APIServer.Models.Entity;
using CloudStructures.Structures;
using static APIServer.LoggerManager;

namespace APIServer.Repository.Implements.Memory;

partial class MemoryDb
{
    public async Task<Result> DeleteCachedUserGameData(long userId)
    {
        var key = CreateUserGameDataKey(userId);
        try
        {
            var handler = new RedisString<UserGameData>(_conn, key, null);

            _ = await handler.DeleteAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.DeleteCacheData, 
                "Failed Delete Cached User Game Data", new { userId, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedCacheGameData);       
        }
    }

    public async Task<Result<UserGameData>> GetCachedUserGameData(long userId)
    {
        var key = CreateUserGameDataKey(userId);
        try
        {
            var handler = new RedisString<UserGameData>(_conn, key, null);
            
            var result = await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<UserGameData>.Success(result.Value);
            }
            
            return Result<UserGameData>.Failure(ErrorCode.CannotFindUserGameData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Get Cached User Game Data", new {userId, ex.Message, ex.StackTrace});
            return Result<UserGameData>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<UserQuestInprogress>>> GetCachedQuestList(long userId)
    {
        var key = CreateQuestKey(userId);
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            var result =  await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<List<UserQuestInprogress>>.Success(result.Value);
            }
            
            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.CannotFindQuestList);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { userId, ex.Message, ex.StackTrace });

            return Result<List<UserQuestInprogress>>.Failure(ErrorCode.FailedCacheGameData);
        }     
    }

    public async Task<Result> CacheQuestList(long userId, List<UserQuestInprogress> progressList)
    {
        var key = CreateQuestKey(userId);
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            var result = await handler.SetAsync(progressList, TimeSpan.FromMinutes(60));
            if (result == false)
            {
                return Result.Failure(ErrorCode.FailedCacheGameData);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result> DeleteCachedQuestList(long userId)
    {
        var key = CreateQuestKey(userId);   
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            _ =  await handler.DeleteAsync();
            return Result.Success();
            
        }catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.DeleteCacheData, 
                "Failed Delete Cached Quest List", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }
    
    public async Task<Result<List<CharacterData>>> GetCachedCharacterDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);
        try
        {
            var handler = new RedisString<List<CharacterData>>(_conn, key, null);

            var result = await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<List<CharacterData>>.Success(result.Value);
            }

            return Result<List<CharacterData>>.Failure(ErrorCode.CannotFindCharacterData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Get Character Data List", new { userId, ex.Message, ex.StackTrace });

            return Result<List<CharacterData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<ItemData>>> GetCachedItemDataList(long userId)
    {
        var key = CreateItemDataKey(userId);
        try
        {
            var handler = new RedisString<List<ItemData>>(_conn, key, null);

            var result = await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<List<ItemData>>.Success(result.Value);
            }

            return Result<List<ItemData>>.Failure(ErrorCode.CannotFindItemData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Get Item Data List", new { userId, ex.Message, ex.StackTrace });

            return Result<List<ItemData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result<List<RuneData>>> GetCachedRuneDataList(long userId)
    {
        var key = CreateRuneDataKey(userId);
        try
        {
            var handler = new RedisString<List<RuneData>>(_conn, key, null);

            var result = await handler.GetAsync();
            if (result.HasValue)
            {
                return Result<List<RuneData>>.Success(result.Value);
            }

            return Result<List<RuneData>>.Failure(ErrorCode.CannotFindRuneData);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Get Rune Data List", new { userId, ex.Message, ex.StackTrace });

            return Result<List<RuneData>>.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<bool> CacheUserGameData(long userId, UserGameData gameData)
    {
        var key = CreateUserGameDataKey(userId);
        try
        {
            var handler = new RedisString<UserGameData>(_conn, key, null);
            
            return await handler.SetAsync(gameData, TimeSpan.FromMinutes(60));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Cache Game Data Failed", new { email = userId, ex.Message, ex.StackTrace });

            return false;
        }
    }
    
    public async Task<bool> CacheCharacterDataList(long userId, List<CharacterData> characterDataList)
    {
        var key = CreateCharacterDataKey(userId);
        try
        {
            var handler = new RedisString<List<CharacterData>>(_conn, key, null);
            
            return await handler.SetAsync(characterDataList, TimeSpan.FromMinutes(60));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Cache Character Data", new { email = userId, ex.Message, ex.StackTrace });

            return false;
        }
    }

    public async Task<bool> CacheItemDataList(long userId, List<ItemData> itemDataList)
    {
        var key = CreateItemDataKey(userId);
        try
        {
            var handler = new RedisString<List<ItemData>>(_conn, key, null);
            
            return await handler.SetAsync(itemDataList, TimeSpan.FromMinutes(60));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Cache Item data", new { email = userId, ex.Message, ex.StackTrace });

            return false;
        }
    }

    public async Task<bool> CacheRuneDataList(long userId, List<RuneData> runeDataList)
    {
        var key = CreateRuneDataKey(userId);
        try
        {
            var handler = new RedisString<List<RuneData>>(_conn, key, null);
            
            return await handler.SetAsync(runeDataList, TimeSpan.FromMinutes(60));
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.CacheGameData, 
                "Failed Cache Rune Data", new { email = userId, ex.Message, ex.StackTrace });

            return false;
        }
    }

    public async Task<Result> DeleteCachedCharacterDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);   
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            _ = await handler.DeleteAsync();
            return Result.Success();
            
        }catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.DeleteCacheData, 
                "Failed Delete Cached Character Data List", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result> DeleteCachedItemDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);   
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            _ = await handler.DeleteAsync();
            return Result.Success();
            
        }catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.DeleteCacheData, 
                "Failed Delete Cached Item Data List", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result> DeleteCachedRuneDataList(long userId)
    {
        var key = CreateCharacterDataKey(userId);   
        try
        {
            var handler = new RedisString<List<UserQuestInprogress>>(_conn, key, null);
            
            _ = await handler.DeleteAsync();
            return Result.Success();
            
        }catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedCacheGameData, EventType.DeleteCacheData, 
                "Failed Delete Cached Rune Data List", new { userId, ex.Message, ex.StackTrace });

            return Result.Failure(ErrorCode.FailedCacheGameData);
        }
    }

    public async Task<Result> DeleteCacheData(long userId, List<CacheType> cacheTypeList)
    {
        var result = Result.Success();
        foreach (var type in cacheTypeList)
        {
            switch (type)
            {
                case CacheType.Character:
                    result =  await DeleteCachedCharacterDataList(userId);
                    break;
                case CacheType.Item:
                    result =  await DeleteCachedItemDataList(userId);
                    break;
                case CacheType.Rune:
                    result =  await DeleteCachedRuneDataList(userId);
                    break;
                case CacheType.Quest:
                    result =  await DeleteCachedQuestList(userId);
                    break;
                case CacheType.UserGameData:
                    result =  await DeleteCachedUserGameData(userId);
                    break;
            }
            
            if(result.IsFailed)
                return result;
        }

        return Result.Success();
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