using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APIServer.Models.DTO.Chat;          // ChatMessage
using APIServer.Repository;               // IMemoryDb
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
    private readonly Mock<ILogger<ChatService>> _logger = new();

    private ChatService Sut() => new(_logger.Object, _memoryDb.Object);

    // =====================================================================
    // SendChatAsync
    // =====================================================================

    /*
     * Target   : SendChatAsync
     * Scenario : 정상 전송
     * Given    : IMemoryDb.SendChatAsync 가 ChatMessage 반환
     * When     : SendChatAsync(email, message)
     * Then     : result.IsSuccess = true, 반환 값 검증 및 MemoryDb 호출 1회
     */
    [Fact(DisplayName = "[Chat][Send] 정상 전송 시 MessageId/필드 반환")]
    [Trait("Target", "SendChatAsync")]
    public async Task SendChatAsync_Case01_Success()
    {
        // Given
        const string email = "user@test.com";
        const string text = "hello";
        var now = DateTime.UtcNow;
        var expected = new ChatMessage
        {
            messageId = "1727920200001-0",
            email = email,
            sendAt = now,
            message = text
        };
        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync(expected);
        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should().BeEquivalentTo(expected);
        _memoryDb.Verify(m => m.SendChatAsync(email, text), Times.Once);
        _memoryDb.VerifyNoOtherCalls();
    }

    /*
     * Target   : SendChatAsync
     * Scenario : MemoryDb 가 null 반환(쓰기 실패)
     * Given    : IMemoryDb.SendChatAsync => null
     * When     : SendChatAsync
     * Then     : result = RedisException
     */
    [Fact(DisplayName = "[Chat][Send] MemoryDb 실패 시 RedisException 반환")]
    [Trait("Target", "SendChatAsync")]
    public async Task SendChatAsync_Case03_MemoryDbNull()
    {
        // Given
        const string email = "user@test.com";
        const string text = "hi";
        _memoryDb.Setup(m => m.SendChatAsync(email, text)).ReturnsAsync((ChatMessage?)null);
        var sut = Sut();

        // When
        var result = await sut.SendAsync(email, text);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedSendChat);
        _memoryDb.Verify(m => m.SendChatAsync(email, text), Times.Once);
    }

    // =====================================================================
    // FetchChatsAsync
    // =====================================================================

    /*
     * Target   : FetchChatsAsync
     * Scenario : 초기 진입 (after = null) → 최신 N개 오름차순으로 반환
     * Given    : IMemoryDb.FetchChatsAsync 가 리스트 반환
     * When     : FetchChatsAsync(count, null)
     * Then     : result.IsSuccess = true, 리스트 개수/정렬/필드 검증
     */
    [Fact(DisplayName = "[Chat][Fetch] 초기 진입 시 최신 N개(오름차순) 반환")]
    [Trait("Target", "FetchChatsAsync")]
    public async Task FetchChatsAsync_Case01_Initial()
    {
        // Given
        const int count = 3;
        var msgs = new List<ChatMessage>
        {
            new() { messageId = "1-0", email="a@a.com", sendAt=DateTime.UtcNow.AddSeconds(-2), message="a" },
            new() { messageId = "2-0", email="b@b.com", sendAt=DateTime.UtcNow.AddSeconds(-1), message="b" },
            new() { messageId = "3-0", email="c@c.com", sendAt=DateTime.UtcNow,             message="c" },
        };
        _memoryDb.Setup(m => m.FetchChatsAsync(count, null)).ReturnsAsync(msgs);
        var sut = Sut();

        // When
        var result = await sut.FetchAsync(count, null);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should().HaveCount(3);
        result.Value![0].messageId.Should().Be("1-0");
        result.Value![2].messageId.Should().Be("3-0");
        _memoryDb.Verify(m => m.FetchChatsAsync(count, null), Times.Once);
        _memoryDb.VerifyNoOtherCalls();
    }

    /*
     * Target   : FetchChatsAsync
     * Scenario : after 커서 지정 → 신규 메시지만 반환
     * Given    : IMemoryDb.FetchChatsAsync 가 리스트 반환
     * When     : FetchChatsAsync(count, after)
     * Then     : result.IsSuccess = true, 메세지들의 messageId가 after보다 큰지 확인
     */
    [Fact(DisplayName = "[Chat][Fetch] 커서 이후 신규 메시지 반환")]
    [Trait("Target", "FetchChatsAsync")]
    public async Task FetchChatsAsync_Case02_AfterCursor()
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
     * Target   : FetchChatsAsync
     * Scenario : MemoryDb 예외 발생
     * Given    : FetchChatsAsync 가 예외 throw
     * When     : FetchChatsAsync
     * Then     : result = RedisException
     */
    [Fact(DisplayName = "[Chat][Fetch] MemoryDb 예외 시 RedisException 반환")]
    [Trait("Target", "FetchChatsAsync")]
    public async Task FetchChatsAsync_Case03_Exception()
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

    /*
     * Target   : FetchChatsAsync
     * Scenario : 빈 결과 (신규 메시지 없음)
     * Given    : FetchChatsAsync 가 빈 리스트 반환
     * When     : FetchChatsAsync
     * Then     : result.IsSuccess = true, 빈 리스트 반환
     */
    [Fact(DisplayName = "[Chat][Fetch] 신규 메시지 없을 때 빈 리스트 성공 반환")]
    [Trait("Target", "FetchChatsAsync")]
    public async Task FetchChatsAsync_Case04_EmptyList()
    {
        // Given
        _memoryDb.Setup(m => m.FetchChatsAsync(100, "100-0")).ReturnsAsync(new List<ChatMessage>());
        var sut = Sut();

        // When
        var result = await sut.FetchAsync(100, "100-0");

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(0);
    }
}
