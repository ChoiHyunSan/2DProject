using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APIServer.Models.DTO.Chat;          // ChatMessage
using APIServer.Models.Entity;            // ChatLog
using APIServer.Repository;               // IMemoryDb, IGameDb
using APIServer.Service;                  // IChatService
using APIServer.Service.Implements;       // ChatService
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace APIServer.Tests.Service;

public class ChatServiceTests
{
    private readonly Mock<IMemoryDb> _memoryDb = new();
    private readonly Mock<IGameDb> _gameDb = new();
    private readonly Mock<ILogger<ChatService>> _logger = new();

    private ChatService Sut() => new(_logger.Object, _memoryDb.Object, _gameDb.Object);

    // =====================================================================
    // SendAsync
    // =====================================================================

    /*
     * Target   : SendAsync
     * Scenario : Redis 성공, DB 적재 성공
     * Given    : IMemoryDb.SendChatAsync -> ChatMessage, IGameDb.InsertChatLogAsync -> true
     * When     : SendAsync(email, message)
     * Then     : IsSuccess=true, ErrorCode=None, GameDb 1회 호출
     */
    [Fact(DisplayName = "[Chat][Send] Redis 성공 + DB 성공 → Success(None)")]
    [Trait("Target", "SendAsync")]
    public async Task SendAsync_Success_When_RedisOk_And_DbOk()
    {
        // Given
        const string email = "user@test.com";
        const string text  = "hello";
        const string msgId = "1727920200001-0";

        var sent = new ChatMessage
        {
            messageId = msgId,
            email     = email,
            sendAt    = DateTime.UtcNow,
            message   = text
        };

        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync(sent);
        _gameDb.Setup(g => g.InsertChatLogAsync(It.IsAny<ChatLog>())).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value!.messageId.Should().Be(msgId);

        // 저장 자체를 검증하진 않되, 호출 여부만 확인
        _gameDb.Verify(g => g.InsertChatLogAsync(It.IsAny<ChatLog>()), Times.Once);
    }

    /*
     * Target   : SendAsync
     * Scenario : Redis 실패(null)
     * Given    : IMemoryDb.SendChatAsync -> null
     * When     : SendAsync(email, message)
     * Then     : FailedSendChat, GameDb 호출 없음
     */
    [Fact(DisplayName = "[Chat][Send] Redis 실패(null) → FailedSendChat")]
    [Trait("Target", "SendAsync")]
    public async Task SendAsync_Fail_When_RedisNull()
    {
        // Given
        const string email = "user@test.com";
        const string text  = "hello";
        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync((ChatMessage?)null);

        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedSendChat);
        _gameDb.Verify(g => g.InsertChatLogAsync(It.IsAny<ChatLog>()), Times.Never);
    }

    /*
     * Target   : SendAsync
     * Scenario : DB 적재 false 반환
     * Given    : InsertChatLogAsync -> false
     * When     : SendAsync
     * Then     : FailedSendChat
     */
    [Fact(DisplayName = "[Chat][Send] DB false → FailedSendChat")]
    [Trait("Target", "SendAsync")]
    public async Task SendAsync_Fail_When_DbReturnsFalse()
    {
        // Given
        const string email = "user@test.com";
        const string text  = "hello";
        var sent = new ChatMessage
        {
            messageId = "1727920200001-0",
            email = email,
            sendAt = DateTime.UtcNow,
            message = text
        };

        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync(sent);
        _gameDb.Setup(g => g.InsertChatLogAsync(It.IsAny<ChatLog>())).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedSendChat);
        _gameDb.Verify(g => g.InsertChatLogAsync(It.IsAny<ChatLog>()), Times.Once);
    }

    /*
     * Target   : SendAsync
     * Scenario : DB 일반 예외 → FailedSendChat
     * Given    : InsertChatLogAsync -> throw new Exception("DB down")
     * When     : SendAsync
     * Then     : FailedSendChat
     */
    [Fact(DisplayName = "[Chat][Send] DB 예외 → FailedSendChat")]
    [Trait("Target", "SendAsync")]
    public async Task SendAsync_Fail_When_DbThrows()
    {
        // Given
        const string email = "user@test.com";
        const string text  = "hello";
        var sent = new ChatMessage
        {
            messageId = "1727920200001-0",
            email = email,
            sendAt = DateTime.UtcNow,
            message = text
        };

        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync(sent);
        _gameDb.Setup(g => g.InsertChatLogAsync(It.IsAny<ChatLog>()))
               .ThrowsAsync(new Exception("DB down"));

        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedSendChat);
        _gameDb.Verify(g => g.InsertChatLogAsync(It.IsAny<ChatLog>()), Times.Once);
    }

    // =====================================================================
    // FetchAsync  (Redis 전용)
    // =====================================================================

    /*
     * Target   : FetchAsync
     * Scenario : 초기 진입( after = null ) → 메시지 N개 오름차순
     * Given    : IMemoryDb.FetchChatsAsync -> 리스트
     * When     : FetchAsync(count, null)
     * Then     : Success(None), 리스트 그대로 반환
     */
    [Fact(DisplayName = "[Chat][Fetch] 초기 진입 → 최신 N개(오름차순)")]
    [Trait("Target", "FetchAsync")]
    public async Task FetchAsync_Initial_ReturnsAscendingList()
    {
        // Given
        const int count = 3;
        var msgs = new List<ChatMessage>
        {
            new() { messageId = "1-0", email="a@a.com", sendAt=DateTime.UtcNow.AddSeconds(-3), message="a" },
            new() { messageId = "2-0", email="b@b.com", sendAt=DateTime.UtcNow.AddSeconds(-2), message="b" },
            new() { messageId = "3-0", email="c@c.com", sendAt=DateTime.UtcNow.AddSeconds(-1), message="c" },
        };
        _memoryDb.Setup(m => m.FetchChatsAsync(count, null)).ReturnsAsync(msgs);

        var sut = Sut();

        // When
        var result = await sut.FetchAsync(count, null);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should().BeEquivalentTo(msgs);
        _memoryDb.Verify(m => m.FetchChatsAsync(count, null), Times.Once);
        _gameDb.VerifyNoOtherCalls(); // Fetch는 DB에 접근하지 않음
    }

    /*
     * Target   : FetchAsync
     * Scenario : after 지정 → 신규 메시지만 반환
     * Given    : IMemoryDb.FetchChatsAsync -> 리스트
     * When     : FetchAsync(count, after)
     * Then     : Success(None)
     */
    [Fact(DisplayName = "[Chat][Fetch] after 커서 이후 메시지 반환")]
    [Trait("Target", "FetchAsync")]
    public async Task FetchAsync_After_ReturnsOnlyNewer()
    {
        // Given
        const int count = 50;
        const string after = "10-0";
        var msgs = new List<ChatMessage>
        {
            new() { messageId = "11-0", email="a@a.com", sendAt=DateTime.UtcNow, message="a" },
            new() { messageId = "12-0", email="b@b.com", sendAt=DateTime.UtcNow, message="b" }
        };
        _memoryDb.Setup(m => m.FetchChatsAsync(count, after)).ReturnsAsync(msgs);

        var sut = Sut();

        // When
        var result = await sut.FetchAsync(count, after);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should().BeEquivalentTo(msgs);
        _memoryDb.Verify(m => m.FetchChatsAsync(count, after), Times.Once);
    }

    /*
     * Target   : FetchAsync
     * Scenario : 빈 결과
     * Given    : IMemoryDb.FetchChatsAsync -> []
     * When     : FetchAsync
     * Then     : Success(None), 빈 리스트
     */
    [Fact(DisplayName = "[Chat][Fetch] 신규 메시지 없음 → 빈 리스트")]
    [Trait("Target", "FetchAsync")]
    public async Task FetchAsync_EmptyList_IsSuccess()
    {
        // Given
        _memoryDb.Setup(m => m.FetchChatsAsync(100, "100-0"))
                 .ReturnsAsync(new List<ChatMessage>());

        var sut = Sut();

        // When
        var result = await sut.FetchAsync(100, "100-0");

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should()!.NotBeNull();
        result.Value!.Count.Should().Be(0);
    }

    /*
     * Target   : FetchAsync
     * Scenario : MemoryDb 예외
     * Given    : IMemoryDb.FetchChatsAsync -> throw
     * When     : FetchAsync
     * Then     : RedisException
     */
    [Fact(DisplayName = "[Chat][Fetch] Redis 예외 → RedisException")]
    [Trait("Target", "FetchAsync")]
    public async Task FetchAsync_Exception_ReturnsRedisException()
    {
        // Given
        _memoryDb.Setup(m => m.FetchChatsAsync(It.IsAny<int>(), It.IsAny<string?>()))
                 .ThrowsAsync(new Exception("Redis down"));

        var sut = Sut();

        // When
        var result = await sut.FetchAsync(20, "5-0");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedFetchChat);
    }
}
