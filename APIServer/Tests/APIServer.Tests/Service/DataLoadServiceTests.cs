using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Repository;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace APIServer.Tests.Service;

public class DataLoadServiceTests
{
    private readonly Mock<IGameDb> _gameDb = new();
    private readonly Mock<IMemoryDb> _memoryDb = new();
    private readonly Mock<ILogger<DataLoadService>> _logger = new();

    private DataLoadService Sut() => new(_logger.Object, _gameDb.Object, _memoryDb.Object);

    private static Pageable P(int page = 1, int size = 10) => new Pageable { page = page, size = size };

    // ----------------------------------------------------------------------
    // LoadGameDataAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : LoadGameDataAsync
     * Scenario : DB에서 전체 게임 데이터 조회에 성공
     * Given    : GameDb.GetAllGameDataByUserIdAsync가 유효 데이터 반환
     * When     : LoadGameDataAsync 호출
     * Then     : 성공(Result.Success)과 함께 해당 데이터 반환
     */
    [Fact(DisplayName = "[DataLoad] 전체 게임 데이터 로드: 정상")]
    [Trait("Target", "LoadGameDataAsync")]
    public async Task LoadGameDataAsync_Case01()
    {
        // Given
        var userId = 10L;
        var data = new FullGameData();
        _gameDb.Setup(g => g.GetAllGameDataByUserIdAsync(userId)).ReturnsAsync(data);

        var sut = Sut();

        // When
        var result = await sut.LoadGameDataAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(data);
    }

    // ----------------------------------------------------------------------
    // GetProgressQuestListAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetProgressQuestListAsync
     * Scenario : 캐시에 진행 퀘스트가 존재
     * Given    : MemoryDb.GetCachedQuestList가 Success(List) 반환
     * When     : GetProgressQuestListAsync 호출
     * Then     : 페이지네이션 결과를 성공으로 반환
     */
    [Fact(DisplayName = "[DataLoad] 진행 퀘스트: 캐시 히트")]
    [Trait("Target", "GetProgressQuestListAsync")]
    public async Task GetProgressQuestListAsync_Case01()
    {
        // Given
        var userId = 1L;
        var pageable = P(page: 1, size: 2);
        var cached = new List<UserQuestInprogress>
        {
            new() { quest_inprogress_id = 1, quest_code = 101, progress = 5,  expire_date = DateTime.UtcNow.AddDays(1)},
            new() { quest_inprogress_id = 2, quest_code = 102, progress = 10, expire_date = DateTime.UtcNow.AddDays(1)},
            new() { quest_inprogress_id = 3, quest_code = 103, progress = 15, expire_date = DateTime.UtcNow.AddDays(1)},
        };
        _memoryDb.Setup(m => m.GetCachedQuestList(userId))
                 .ReturnsAsync(Result<List<UserQuestInprogress>>.Success(cached));

        var sut = Sut();

        // When
        var result = await sut.GetProgressQuestListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cached.Take(2).ToList());
        _gameDb.Verify(g => g.GetProgressQuestList(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : GetProgressQuestListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 성공
     * Given    : GetCachedQuestList=Failure, GetProgressQuestList=List, CacheQuestList=Success
     * When     : GetProgressQuestListAsync 호출
     * Then     : 페이지네이션 결과를 성공으로 반환
     */
    [Fact(DisplayName = "[DataLoad] 진행 퀘스트: 캐시 미스 → DB 조회 + 캐시 OK")]
    [Trait("Target", "GetProgressQuestListAsync")]
    public async Task GetProgressQuestListAsync_Case02()
    {
        // Given
        var userId = 2L;
        var pageable = P(1, 3);
        var fromDb = new List<UserQuestInprogress>
        {
            new() { quest_inprogress_id = 1, quest_code = 201, progress = 1, expire_date = DateTime.UtcNow.AddDays(1)},
            new() { quest_inprogress_id = 2, quest_code = 202, progress = 2, expire_date = DateTime.UtcNow.AddDays(1)},
            new() { quest_inprogress_id = 3, quest_code = 203, progress = 3, expire_date = DateTime.UtcNow.AddDays(1)},
            new() { quest_inprogress_id = 4, quest_code = 204, progress = 4, expire_date = DateTime.UtcNow.AddDays(1)},
        };

        _memoryDb.Setup(m => m.GetCachedQuestList(userId))
                 .ReturnsAsync(Result<List<UserQuestInprogress>>.Failure(ErrorCode.CannotFindQuestList));
        _gameDb.Setup(g => g.GetProgressQuestList(userId)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheQuestList(userId, fromDb))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.GetProgressQuestListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(fromDb.Take(3).ToList());
        _memoryDb.Verify(m => m.CacheQuestList(userId, fromDb), Times.Once);
    }

    /*
     * Target   : GetProgressQuestListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 실패
     * Given    : CacheQuestList=Failure(FailedCacheGameData)
     * When     : GetProgressQuestListAsync 호출
     * Then     : FailedCacheGameData 실패 반환
     */
    [Fact(DisplayName = "[DataLoad] 진행 퀘스트: 캐시 미스 → DB 조회 + 캐시 실패")]
    [Trait("Target", "GetProgressQuestListAsync")]
    public async Task GetProgressQuestListAsync_Case03()
    {
        // Given
        var userId = 3L;
        var pageable = P(1, 10);
        var fromDb = new List<UserQuestInprogress> { new() { quest_inprogress_id = 1, quest_code = 301, progress = 0, expire_date = DateTime.UtcNow.AddDays(1)} };

        _memoryDb.Setup(m => m.GetCachedQuestList(userId))
                 .ReturnsAsync(Result<List<UserQuestInprogress>>.Failure(ErrorCode.CannotFindQuestList));
        _gameDb.Setup(g => g.GetProgressQuestList(userId)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheQuestList(userId, fromDb))
                 .ReturnsAsync(Result.Failure(ErrorCode.FailedCacheGameData));

        var sut = Sut();

        // When
        var result = await sut.GetProgressQuestListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }

    // ----------------------------------------------------------------------
    // GetCompleteQuestListAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetCompleteQuestListAsync
     * Scenario : DB에서 완료 퀘스트 페이지 조회
     * Given    : GameDb.GetCompleteQuestList가 List 반환
     * When     : GetCompleteQuestListAsync 호출
     * Then     : 성공과 함께 List 반환
     */
    [Fact(DisplayName = "[DataLoad] 완료 퀘스트: DB 페이징 조회")]
    [Trait("Target", "GetCompleteQuestListAsync")]
    public async Task GetCompleteQuestListAsync_Case01()
    {
        // Given
        var userId = 5L;
        var pageable = P(2, 2);
        var list = new List<UserQuestComplete>
        {
            new() { quest_complete_id = 11, quest_code = 501, complete_date = DateTime.UtcNow, earn_reward = false },
            new() { quest_complete_id = 12, quest_code = 502, complete_date = DateTime.UtcNow, earn_reward = true  },
        };

        _gameDb.Setup(g => g.GetCompleteQuestList(userId, pageable)).ReturnsAsync(list);

        var sut = Sut();

        // When
        var result = await sut.GetCompleteQuestListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(list);
    }

    // ----------------------------------------------------------------------
    // GetInventoryCharacterListAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetInventoryCharacterListAsync
     * Scenario : 캐시에 캐릭터 리스트 존재
     * Given    : GetCachedCharacterDataList=Success(List)
     * When     : GetInventoryCharacterListAsync 호출
     * Then     : 성공과 함께 캐시 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 캐릭터 인벤토리: 캐시 히트")]
    [Trait("Target", "GetInventoryCharacterListAsync")]
    public async Task GetInventoryCharacterListAsync_Case01()
    {
        // Given
        var userId = 7L;
        var cached = new List<CharacterData> { new() { characterId = 1, characterCode = 30001, level = 1 } };

        _memoryDb.Setup(m => m.GetCachedCharacterDataList(userId))
                 .ReturnsAsync(Result<List<CharacterData>>.Success(cached));

        var sut = Sut();

        // When
        var result = await sut.GetInventoryCharacterListAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(cached);
        _gameDb.Verify(g => g.GetCharacterDataListAsync(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : GetInventoryCharacterListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 성공
     * Given    : GetCachedCharacterDataList=Failure, CacheCharacterDataList=true
     * When     : GetInventoryCharacterListAsync 호출
     * Then     : 성공과 함께 DB 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 캐릭터 인벤토리: 캐시 미스 → DB 조회 + 캐시 OK")]
    [Trait("Target", "GetInventoryCharacterListAsync")]
    public async Task GetInventoryCharacterListAsync_Case02()
    {
        // Given
        var userId = 8L;
        var fromDb = new List<CharacterData> { new() { characterId = 2, characterCode = 30002, level = 5 } };

        _memoryDb.Setup(m => m.GetCachedCharacterDataList(userId))
                 .ReturnsAsync(Result<List<CharacterData>>.Failure(ErrorCode.CannotFindCharacterData));
        _gameDb.Setup(g => g.GetCharacterDataListAsync(userId)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheCharacterDataList(userId, fromDb)).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryCharacterListAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(fromDb);
        _memoryDb.Verify(m => m.CacheCharacterDataList(userId, fromDb), Times.Once);
    }

    /*
     * Target   : GetInventoryCharacterListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 실패
     * Given    : CacheCharacterDataList=false
     * When     : GetInventoryCharacterListAsync 호출
     * Then     : FailedCacheGameData 실패 반환
     */
    [Fact(DisplayName = "[DataLoad] 캐릭터 인벤토리: 캐시 미스 → DB 조회 + 캐시 실패")]
    [Trait("Target", "GetInventoryCharacterListAsync")]
    public async Task GetInventoryCharacterListAsync_Case03()
    {
        // Given
        var userId = 9L;
        var fromDb = new List<CharacterData> { new() { characterId = 3, characterCode = 30003, level = 7 } };

        _memoryDb.Setup(m => m.GetCachedCharacterDataList(userId))
                 .ReturnsAsync(Result<List<CharacterData>>.Failure(ErrorCode.CannotFindCharacterData));
        _gameDb.Setup(g => g.GetCharacterDataListAsync(userId)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheCharacterDataList(userId, fromDb)).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryCharacterListAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }

    // ----------------------------------------------------------------------
    // GetInventoryItemListAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetInventoryItemListAsync
     * Scenario : 캐시에 아이템 리스트 존재
     * Given    : GetCachedItemDataList=Success(List)
     * When     : GetInventoryItemListAsync 호출
     * Then     : 성공과 함께 캐시 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 아이템 인벤토리: 캐시 히트")]
    [Trait("Target", "GetInventoryItemListAsync")]
    public async Task GetInventoryItemListAsync_Case01()
    {
        // Given
        var userId = 11L;
        var pageable = P(1, 20);
        var cached = new List<ItemData> { new() { itemId = 1, itemCode = 10001, level = 1 } };

        _memoryDb.Setup(m => m.GetCachedItemDataList(userId))
                 .ReturnsAsync(Result<List<ItemData>>.Success(cached));

        var sut = Sut();

        // When
        var result = await sut.GetInventoryItemListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(cached);
        _gameDb.Verify(g => g.GetItemDataListAsync(It.IsAny<long>(), It.IsAny<Pageable>()), Times.Never);
    }

    /*
     * Target   : GetInventoryItemListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 성공
     * Given    : GetCachedItemDataList=Failure, CacheItemDataList=true
     * When     : GetInventoryItemListAsync 호출
     * Then     : 성공과 함께 DB 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 아이템 인벤토리: 캐시 미스 → DB 조회 + 캐시 OK")]
    [Trait("Target", "GetInventoryItemListAsync")]
    public async Task GetInventoryItemListAsync_Case02()
    {
        // Given
        var userId = 12L;
        var pageable = P(1, 5);
        var fromDb = new List<ItemData> { new() { itemId = 2, itemCode = 10002, level = 3 } };

        _memoryDb.Setup(m => m.GetCachedItemDataList(userId))
                 .ReturnsAsync(Result<List<ItemData>>.Failure(ErrorCode.CannotFindItemData));
        _gameDb.Setup(g => g.GetItemDataListAsync(userId, pageable)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheItemDataList(userId, fromDb)).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryItemListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(fromDb);
    }

    /*
     * Target   : GetInventoryItemListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 실패
     * Given    : CacheItemDataList=false
     * When     : GetInventoryItemListAsync 호출
     * Then     : FailedCacheGameData 실패 반환
     */
    [Fact(DisplayName = "[DataLoad] 아이템 인벤토리: 캐시 미스 → DB 조회 + 캐시 실패")]
    [Trait("Target", "GetInventoryItemListAsync")]
    public async Task GetInventoryItemListAsync_Case03()
    {
        // Given
        var userId = 13L;
        var pageable = P(1, 5);
        var fromDb = new List<ItemData> { new() { itemId = 3, itemCode = 10003, level = 2 } };

        _memoryDb.Setup(m => m.GetCachedItemDataList(userId))
                 .ReturnsAsync(Result<List<ItemData>>.Failure(ErrorCode.CannotFindItemData));
        _gameDb.Setup(g => g.GetItemDataListAsync(userId, pageable)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheItemDataList(userId, fromDb)).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryItemListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }

    // ----------------------------------------------------------------------
    // GetInventoryRuneListAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetInventoryRuneListAsync
     * Scenario : 캐시에 룬 리스트 존재
     * Given    : GetCachedRuneDataList=Success(List)
     * When     : GetInventoryRuneListAsync 호출
     * Then     : 성공과 함께 캐시 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 룬 인벤토리: 캐시 히트")]
    [Trait("Target", "GetInventoryRuneListAsync")]
    public async Task GetInventoryRuneListAsync_Case01()
    {
        // Given
        var userId = 21L;
        var pageable = P(1, 20);
        var cached = new List<RuneData> { new() { runeId = 1, runeCode = 20000, level = 1 } };

        _memoryDb.Setup(m => m.GetCachedRuneDataList(userId))
                 .ReturnsAsync(Result<List<RuneData>>.Success(cached));

        var sut = Sut();

        // When
        var result = await sut.GetInventoryRuneListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(cached);
        _gameDb.Verify(g => g.GetRuneDataListAsync(It.IsAny<long>(), It.IsAny<Pageable>()), Times.Never);
    }

    /*
     * Target   : GetInventoryRuneListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 성공
     * Given    : GetCachedRuneDataList=Failure, CacheRuneDataList=true
     * When     : GetInventoryRuneListAsync 호출
     * Then     : 성공과 함께 DB 리스트 반환
     */
    [Fact(DisplayName = "[DataLoad] 룬 인벤토리: 캐시 미스 → DB 조회 + 캐시 OK")]
    [Trait("Target", "GetInventoryRuneListAsync")]
    public async Task GetInventoryRuneListAsync_Case02()
    {
        // Given
        var userId = 22L;
        var pageable = P(2, 5);
        var fromDb = new List<RuneData> { new() { runeId = 2, runeCode = 20001, level = 2 } };

        _memoryDb.Setup(m => m.GetCachedRuneDataList(userId))
                 .ReturnsAsync(Result<List<RuneData>>.Failure(ErrorCode.CannotFindRuneData));
        _gameDb.Setup(g => g.GetRuneDataListAsync(userId, pageable)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheRuneDataList(userId, fromDb)).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryRuneListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(fromDb);
    }

    /*
     * Target   : GetInventoryRuneListAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 실패
     * Given    : CacheRuneDataList=false
     * When     : GetInventoryRuneListAsync 호출
     * Then     : FailedCacheGameData 실패 반환
     */
    [Fact(DisplayName = "[DataLoad] 룬 인벤토리: 캐시 미스 → DB 조회 + 캐시 실패")]
    [Trait("Target", "GetInventoryRuneListAsync")]
    public async Task GetInventoryRuneListAsync_Case03()
    {
        // Given
        var userId = 23L;
        var pageable = P(1, 5);
        var fromDb = new List<RuneData> { new() { runeId = 3, runeCode = 20002, level = 3 } };

        _memoryDb.Setup(m => m.GetCachedRuneDataList(userId))
                 .ReturnsAsync(Result<List<RuneData>>.Failure(ErrorCode.CannotFindRuneData));
        _gameDb.Setup(g => g.GetRuneDataListAsync(userId, pageable)).ReturnsAsync(fromDb);
        _memoryDb.Setup(m => m.CacheRuneDataList(userId, fromDb)).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.GetInventoryRuneListAsync(userId, pageable);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }

    // ----------------------------------------------------------------------
    // GetUserGameDataAsync
    // ----------------------------------------------------------------------

    /*
     * Target   : GetUserGameDataAsync
     * Scenario : 캐시에 유저 게임 데이터 존재
     * Given    : GetCachedUserGameData=Success(data)
     * When     : GetUserGameDataAsync 호출
     * Then     : 성공과 함께 캐시 데이터 반환
     */
    [Fact(DisplayName = "[DataLoad] 유저 게임 데이터: 캐시 히트")]
    [Trait("Target", "GetUserGameDataAsync")]
    public async Task GetUserGameDataAsync_Case01()
    {
        // Given
        var userId = 31L;
        var cached = new UserGameData { level = 10, gold = 123 };

        _memoryDb.Setup(m => m.GetCachedUserGameData(userId))
                 .ReturnsAsync(Result<UserGameData>.Success(cached));

        var sut = Sut();

        // When
        var result = await sut.GetUserGameDataAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(cached);
        _gameDb.Verify(g => g.GetUserGameDataAsync(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : GetUserGameDataAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 성공
     * Given    : GetCachedUserGameData=Failure, CacheUserGameData=true
     * When     : GetUserGameDataAsync 호출
     * Then     : 성공과 함께 DB 데이터 반환
     */
    [Fact(DisplayName = "[DataLoad] 유저 게임 데이터: 캐시 미스 → DB 조회 + 캐시 OK")]
    [Trait("Target", "GetUserGameDataAsync")]
    public async Task GetUserGameDataAsync_Case02()
    {
        // Given
        var userId = 32L;
        var dbData = new UserGameData { level = 3, gold = 999 };

        _memoryDb.Setup(m => m.GetCachedUserGameData(userId))
                 .ReturnsAsync(Result<UserGameData>.Failure(ErrorCode.CannotFindUserGameData));
        _gameDb.Setup(g => g.GetUserGameDataAsync(userId)).ReturnsAsync(dbData);
        _memoryDb.Setup(m => m.CacheUserGameData(userId, dbData)).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.GetUserGameDataAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dbData);
    }

    /*
     * Target   : GetUserGameDataAsync
     * Scenario : 캐시 미스, DB 조회 후 캐시 실패
     * Given    : CacheUserGameData=false
     * When     : GetUserGameDataAsync 호출
     * Then     : FailedCacheGameData 실패 반환
     */
    [Fact(DisplayName = "[DataLoad] 유저 게임 데이터: 캐시 미스 → DB 조회 + 캐시 실패")]
    [Trait("Target", "GetUserGameDataAsync")]
    public async Task GetUserGameDataAsync_Case03()
    {
        // Given
        var userId = 33L;
        var dbData = new UserGameData { level = 1, gold = 1 };

        _memoryDb.Setup(m => m.GetCachedUserGameData(userId))
                 .ReturnsAsync(Result<UserGameData>.Failure(ErrorCode.CannotFindUserGameData));
        _gameDb.Setup(g => g.GetUserGameDataAsync(userId)).ReturnsAsync(dbData);
        _memoryDb.Setup(m => m.CacheUserGameData(userId, dbData)).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.GetUserGameDataAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }
}
