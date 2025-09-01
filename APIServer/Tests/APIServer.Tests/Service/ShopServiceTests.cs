using System.Collections.Immutable;
using APIServer;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;
using Xunit;

namespace APIServer.Tests.Service;

public class ShopServiceTests
{
    private readonly Mock<IMasterDb> _master = new();
    private readonly Mock<IGameDb> _game = new();
    private readonly Mock<IMemoryDb> _memory = new();
    private readonly Mock<ILogger<ShopService>> _logger = new();

    private ShopService Sut() => new(_logger.Object, _master.Object, _game.Object, _memory.Object);

    private static ImmutableDictionary<long, CharacterOriginData> CharPrice(long code, int gold, int gem)
        => ImmutableDictionary<long, CharacterOriginData>.Empty.Add(code, new CharacterOriginData
        {
            character_code = code, price_gold = gold, price_gem = gem
        });

    private static ImmutableDictionary<(long, int), ItemEnhanceData> ItemEnhance((long,int) key, int sellPrice)
        => ImmutableDictionary<(long,int), ItemEnhanceData>.Empty.Add(key, new ItemEnhanceData
        {
            item_code = key.Item1, level = key.Item2, sell_price = sellPrice
        });

    // ------------------------------------------------------------
    // PurchaseCharacterAsync
    // ------------------------------------------------------------

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : 정상 구매 성공
     * Given    : 보유확인 true, 잔액(100,50), 가격(30,10), TX 모두 성공
     * When     : PurchaseCharacterAsync
     * Then     : None, (70,40) 반환 및 캐시 삭제 1회
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 성공 → 잔액 차감/캐시삭제 OK")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case01()
    {
        const long userId = 1, code = 1001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, gold: 30, gem: 10));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(true);
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((100, 50));

        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 70, 40)).ReturnsAsync(true);
        _game.Setup(g => g.InsertNewCharacterAsync(userId, code)).ReturnsAsync(true);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be((70, 40));
        _memory.Verify(m => m.DeleteCacheData(userId, It.Is<List<CacheType>>(l =>
            l.Contains(CacheType.Character) && l.Contains(CacheType.UserGameData))), Times.Once);
    }

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : 보유확인 실패
     * Given    : CheckAlreadyHaveCharacterAsync == false
     * When     : PurchaseCharacterAsync
     * Then     : CannotFindCharacter
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 실패(보유확인 실패) → CannotFindCharacter")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case02()
    {
        const long userId = 2, code = 2001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, 10, 0));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(false);

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindCharacter);
    }

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : 잔액 부족
     * Given    : 현재(10,0), 가격(30,0)
     * When     : PurchaseCharacterAsync
     * Then     : CannotPurchaseCharacter
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 실패(잔액 부족) → CannotPurchaseCharacter")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case03()
    {
        const long userId = 3, code = 3001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, 30, 0));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(true);
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((10, 0));

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotPurchaseCharacter);
    }

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : TX에서 재화 차감 실패
     * Given    : UpdateUserCurrencyAsync == false
     * When     : PurchaseCharacterAsync
     * Then     : FailedUpdateData, 이후 단계 호출 안됨
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 실패(TX-재화차감) → FailedUpdateData")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case04()
    {
        const long userId = 4, code = 4001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, 10, 0));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(true);
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((50, 0));

        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 40, 0)).ReturnsAsync(false);

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedUpdateData);

        _game.Verify(g => g.InsertNewCharacterAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        _memory.Verify(m => m.DeleteCacheData(It.IsAny<long>(), It.IsAny<List<CacheType>>()), Times.Never);
    }

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : TX에서 캐릭터 추가 실패
     * Given    : InsertNewCharacterAsync == false
     * When     : PurchaseCharacterAsync
     * Then     : FailedInsertNewCharacter
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 실패(TX-캐릭터추가) → FailedInsertNewCharacter")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case05()
    {
        const long userId = 5, code = 5001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, 10, 0));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(true);
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((50, 0));

        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 40, 0)).ReturnsAsync(true);
        _game.Setup(g => g.InsertNewCharacterAsync(userId, code)).ReturnsAsync(false);

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedInsertNewCharacter);

        _memory.Verify(m => m.DeleteCacheData(It.IsAny<long>(), It.IsAny<List<CacheType>>()), Times.Never);
    }

    /*
     * Target   : PurchaseCharacterAsync
     * Scenario : TX에서 캐시 삭제 실패
     * Given    : DeleteCacheData == Failure(FailedCacheGameData)
     * When     : PurchaseCharacterAsync
     * Then     : FailedCacheGameData
     */
    [Fact(DisplayName = "[Shop] 캐릭터 구매 실패(TX-캐시삭제) → FailedCacheGameData")]
    [Trait("Target", "PurchaseCharacterAsync")]
    public async Task PurchaseCharacterAsync_Case06()
    {
        const long userId = 6, code = 6001;
        _master.Setup(m => m.GetCharacterOriginDatas()).Returns(CharPrice(code, 10, 0));
        _game.Setup(g => g.CheckAlreadyHaveCharacterAsync(userId, code)).ReturnsAsync(true);
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((50, 0));

        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 40, 0)).ReturnsAsync(true);
        _game.Setup(g => g.InsertNewCharacterAsync(userId, code)).ReturnsAsync(true);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Failure(ErrorCode.FailedCacheGameData));

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.PurchaseCharacterAsync(userId, code);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }

    // ------------------------------------------------------------
    // SellItemAsync
    // ------------------------------------------------------------

    /*
     * Target   : SellItemAsync
     * Scenario : 장착 아이템은 판매 불가
     * Given    : IsItemEquippedAsync == true
     * When     : SellItemAsync
     * Then     : CannotSellEquipmentItem
     */
    [Fact(DisplayName = "[Shop] 판매 실패(장착중) → CannotSellEquipmentItem")]
    [Trait("Target", "SellItemAsync")]
    public async Task SellItemAsync_Case01()
    {
        const long userId = 10, itemId = 100;
        var inv = new UserInventoryItem { item_id = itemId, item_code = 10001, level = 1 };

        _game.Setup(g => g.GetInventoryItemAsync(userId, itemId)).ReturnsAsync(inv);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(true);

        var sut = Sut();
        var result = await sut.SellItemAsync(userId, itemId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotSellEquipmentItem);
    }

    /*
     * Target   : SellItemAsync
     * Scenario : 정상 판매 성공
     * Given    : 판매가 25, 현재(100,5) → 결과(125,5), TX 성공
     * When     : SellItemAsync
     * Then     : None, DeleteCacheData 1회
     */
    [Fact(DisplayName = "[Shop] 판매 성공 → 재화 증가/캐시삭제 OK")]
    [Trait("Target", "SellItemAsync")]
    public async Task SellItemAsync_Case02()
    {
        const long userId = 11, itemId = 110; int lvl = 2;
        var inv = new UserInventoryItem { item_id = itemId, item_code = 10002, level = lvl };

        _game.Setup(g => g.GetInventoryItemAsync(userId, itemId)).ReturnsAsync(inv);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);

        _master.Setup(m => m.GetItemEnhanceDatas()).Returns(ItemEnhance((itemId, lvl), sellPrice: 25)); // 구현상 (itemId, level) 키 사용
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((100, 5));

        _game.Setup(g => g.DeleteInventoryItemAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 125, 5)).ReturnsAsync(true);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.SellItemAsync(userId, itemId);

        result.IsSuccess.Should().BeTrue();
        _memory.Verify(m => m.DeleteCacheData(userId, It.Is<List<CacheType>>(l =>
            l.Contains(CacheType.Item) && l.Contains(CacheType.UserGameData))), Times.Once);
    }

    /*
     * Target   : SellItemAsync
     * Scenario : TX에서 아이템 삭제 실패
     * Given    : DeleteInventoryItemAsync == false
     * When     : SellItemAsync
     * Then     : FailedDeleteInventoryItem
     */
    [Fact(DisplayName = "[Shop] 판매 실패(TX-아이템삭제) → FailedDeleteInventoryItem")]
    [Trait("Target", "SellItemAsync")]
    public async Task SellItemAsync_Case03()
    {
        const long userId = 12, itemId = 120; int lvl = 1;
        var inv = new UserInventoryItem { item_id = itemId, item_code = 10003, level = lvl };

        _game.Setup(g => g.GetInventoryItemAsync(userId, itemId)).ReturnsAsync(inv);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);
        _master.Setup(m => m.GetItemEnhanceDatas()).Returns(ItemEnhance((itemId, lvl), sellPrice: 10));
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((0, 0));

        _game.Setup(g => g.DeleteInventoryItemAsync(userId, itemId)).ReturnsAsync(false);

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.SellItemAsync(userId, itemId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedDeleteInventoryItem);

        _game.Verify(g => g.UpdateUserCurrencyAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _memory.Verify(m => m.DeleteCacheData(It.IsAny<long>(), It.IsAny<List<CacheType>>()), Times.Never);
    }

    /*
     * Target   : SellItemAsync
     * Scenario : TX에서 재화 갱신 실패
     * Given    : UpdateUserCurrencyAsync == false
     * When     : SellItemAsync
     * Then     : FailedUpdateUserGoldAndGem
     */
    [Fact(DisplayName = "[Shop] 판매 실패(TX-재화갱신) → FailedUpdateUserGoldAndGem")]
    [Trait("Target", "SellItemAsync")]
    public async Task SellItemAsync_Case04()
    {
        const long userId = 13, itemId = 130; int lvl = 1;
        var inv = new UserInventoryItem { item_id = itemId, item_code = 10004, level = lvl };

        _game.Setup(g => g.GetInventoryItemAsync(userId, itemId)).ReturnsAsync(inv);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);
        _master.Setup(m => m.GetItemEnhanceDatas()).Returns(ItemEnhance((itemId, lvl), sellPrice: 5));
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((10, 0));

        _game.Setup(g => g.DeleteInventoryItemAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 15, 0)).ReturnsAsync(false);

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.SellItemAsync(userId, itemId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedUpdateUserGoldAndGem);

        _memory.Verify(m => m.DeleteCacheData(It.IsAny<long>(), It.IsAny<List<CacheType>>()), Times.Never);
    }

    /*
     * Target   : SellItemAsync
     * Scenario : TX에서 캐시 삭제 실패
     * Given    : DeleteCacheData == Failure(FailedCacheGameData)
     * When     : SellItemAsync
     * Then     : FailedCacheGameData
     */
    [Fact(DisplayName = "[Shop] 판매 실패(TX-캐시삭제) → FailedCacheGameData")]
    [Trait("Target", "SellItemAsync")]
    public async Task SellItemAsync_Case05()
    {
        const long userId = 14, itemId = 140; int lvl = 3;
        var inv = new UserInventoryItem { item_id = itemId, item_code = 10005, level = lvl };

        _game.Setup(g => g.GetInventoryItemAsync(userId, itemId)).ReturnsAsync(inv);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);
        _master.Setup(m => m.GetItemEnhanceDatas()).Returns(ItemEnhance((itemId, lvl), sellPrice: 7));
        _game.Setup(g => g.GetUserCurrencyAsync(userId)).ReturnsAsync((1, 1));

        _game.Setup(g => g.DeleteInventoryItemAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.UpdateUserCurrencyAsync(userId, 8, 1)).ReturnsAsync(true);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Failure(ErrorCode.FailedCacheGameData));

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.SellItemAsync(userId, itemId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }
}
