using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using APIServer;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;
using Xunit;


namespace APIServer.Tests.Service;

public class InventoryServiceTests
{
    private readonly Mock<IGameDb> _game = new();
    private readonly Mock<IMasterDb> _master = new();
    private readonly Mock<IMemoryDb> _memory = new();
    private readonly Mock<ILogger<InventoryService>> _logger = new();

    private InventoryService Sut() => new(_logger.Object, _game.Object, _master.Object, _memory.Object);

    /*
     * Target   : EquipItemAsync
     * Scenario : 캐릭터가 존재하지 않음
     * Given    : IsCharacterExistsAsync == false
     * When     : EquipItemAsync(userId, characterId, itemId)
     * Then     : result = CannotFindCharacter
     */
    [Fact(DisplayName = "[Inventory] 캐릭터 미존재 → 장착 실패")]
    [Trait("Target", "EquipItemAsync")]
    public async Task EquipItemAsync_Case01()
    {
        // Given
        const long userId = 1, characterId = 10, itemId = 100;
        _game.Setup(g => g.IsCharacterExistsAsync(userId, characterId)).ReturnsAsync(false);
        var sut = Sut();

        // When
        var result = await sut.EquipItemAsync(userId, characterId, itemId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindCharacter);
    }

    /*
     * Target   : EquipItemAsync
     * Scenario : 아이템이 존재하지 않음
     * Given    : IsItemExistsAsync == false
     * When     : EquipItemAsync(userId, characterId, itemId)
     * Then     : result = CannotFindInventoryItem
     */
    [Fact(DisplayName = "[Inventory] 아이템 미존재 → 장착 실패")]
    [Trait("Target", "EquipItemAsync")]
    public async Task EquipItemAsync_Case02()
    {
        // Given
        const long userId = 1, characterId = 10, itemId = 101;
        _game.Setup(g => g.IsCharacterExistsAsync(userId, characterId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemExistsAsync(userId, itemId)).ReturnsAsync(false);
        var sut = Sut();

        // When
        var result = await sut.EquipItemAsync(userId, characterId, itemId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindInventoryItem);
    }

    /*
     * Target   : EquipItemAsync
     * Scenario : 이미 장착된 아이템
     * Given    : IsItemEquippedAsync == true
     * When     : EquipItemAsync(userId, characterId, itemId)
     * Then     : result = AlreadyEquippedItem
     */
    [Fact(DisplayName = "[Inventory] 이미 장착된 아이템이면 실패")]
    [Trait("Target", "EquipItemAsync")]
    public async Task EquipItemAsync_Case03()
    {
        // Given
        const long userId = 1, characterId = 10, itemId = 102;
        _game.Setup(g => g.IsCharacterExistsAsync(userId, characterId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemExistsAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(true);
        var sut = Sut();

        // When
        var result = await sut.EquipItemAsync(userId, characterId, itemId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.AlreadyEquippedItem);
    }

    /*
     * Target   : EquipItemAsync
     * Scenario : DB 장착 실패
     * Given    : EquipItemAsync(DB) == false
     * When     : EquipItemAsync(userId, characterId, itemId)
     * Then     : result = FailedEquipItem
     */
    [Fact(DisplayName = "[Inventory] DB 장착 실패 시 에러코드")]
    [Trait("Target", "EquipItemAsync")]
    public async Task EquipItemAsync_Case04()
    {
        // Given
        const long userId = 1, characterId = 10, itemId = 103;
        _game.Setup(g => g.IsCharacterExistsAsync(userId, characterId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemExistsAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);
        _game.Setup(g => g.EquipItemAsync(characterId, itemId)).ReturnsAsync(false);
        var sut = Sut();

        // When
        var result = await sut.EquipItemAsync(userId, characterId, itemId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedEquipItem);
    }

    /*
     * Target   : EquipItemAsync
     * Scenario : 정상 장착 및 캐시 삭제
     * Given    : EquipItemAsync == true, DeleteCacheData == Success
     * When     : EquipItemAsync(userId, characterId, itemId)
     * Then     : result = None
     */
    [Fact(DisplayName = "[Inventory] 아이템 장착 성공")]
    [Trait("Target", "EquipItemAsync")]
    public async Task EquipItemAsync_Case05()
    {
        // Given
        const long userId = 1, characterId = 10, itemId = 104;
        _game.Setup(g => g.IsCharacterExistsAsync(userId, characterId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemExistsAsync(userId, itemId)).ReturnsAsync(true);
        _game.Setup(g => g.IsItemEquippedAsync(itemId)).ReturnsAsync(false);
        _game.Setup(g => g.EquipItemAsync(characterId, itemId)).ReturnsAsync(true);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());
        var sut = Sut();

        // When
        var result = await sut.EquipItemAsync(userId, characterId, itemId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
    }
}
