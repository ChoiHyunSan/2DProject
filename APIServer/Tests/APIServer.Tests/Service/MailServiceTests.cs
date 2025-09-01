using APIServer.Models.Entity;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using APIServer.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;


namespace APIServer.Tests.Service;

public class MailServiceTests
{
    private readonly Mock<IGameDb> _game = new();
    private readonly Mock<IMemoryDb> _memory = new();
    private readonly Mock<ILogger<MailService>> _logger = new();

    private MailService Sut() => new(_logger.Object, _game.Object, _memory.Object);

    /*
     * Target   : SendRewardMail
     * Scenario : DB 삽입 실패
     * Given    : InsertNewMail == false
     * When     : SendRewardMail(userId, title, code, count)
     * Then     : result = FailedSendMail
     */
    [Fact(DisplayName = "[Mail] 보상 메일 송신 실패")]
    [Trait("Target", "SendRewardMail")]
    public async Task SendRewardMail_Case01()
    {
        // Given
        const long userId = 1;
        _game.Setup(g => g.InsertNewMail(It.IsAny<UserMail>())).ReturnsAsync(false);
        var sut = Sut();

        // When
        var result = await sut.SendRewardMail(userId, "t", 1, 1);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedSendMail);
    }

    /*
     * Target   : SendRewardMail
     * Scenario : 정상 송신
     * Given    : InsertNewMail == true
     * When     : SendRewardMail(userId, title, code, count)
     * Then     : result = None
     */
    [Fact(DisplayName = "[Mail] 보상 메일 송신 성공")]
    [Trait("Target", "SendRewardMail")]
    public async Task SendRewardMail_Case02()
    {
        // Given
        const long userId = 2;
        _game.Setup(g => g.InsertNewMail(It.IsAny<UserMail>())).ReturnsAsync(true);
        var sut = Sut();

        // When
        var result = await sut.SendRewardMail(userId, "title", 1, 10);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
    }
    
    /*
     * Target   : ReceiveMailAsync
     * Scenario : 메일을 찾을 수 없음
     * Given    : GetMailAsync(userId, mailId) == null
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = CannotFindMail
     */
    [Fact(DisplayName = "[Mail] 메일 없음 → 수령 실패")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case01()
    {
        // Given
        const long userId = 1, mailId = 100;
        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync((UserMail)null!);
        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindMail);
    }

    /*
     * Target   : ReceiveMailAsync
     * Scenario : 캐시 삭제 실패 → 보상 처리 실패
     * Given    : reward_code = 아이템(10000대), DeleteCacheData == Failure
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = FailedReceiveMail, ReceiveCompleteMailAsync 호출 안됨
     */
    [Fact(DisplayName = "[Mail] 캐시 삭제 실패 → 수령 실패")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case02()
    {
        // Given
        const long userId = 2, mailId = 200, itemCode = 10001; // 아이템 영역(10000대)
        var mail = new UserMail { mail_id = mailId, user_id = userId, reward_code = itemCode, count = 1 };

        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync(mail);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Failure(ErrorCode.FailedCacheGameData));

        // 트랜잭션 델리게이트 실제 실행
        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedReceiveMail);
        _game.Verify(g => g.ReceiveCompleteMailAsync(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : ReceiveMailAsync
     * Scenario : 아이템 보상 수령 성공
     * Given    : reward_code = 아이템(10000대), InsertItemAsync == true, ReceiveCompleteMailAsync == true
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = None
     */
    [Fact(DisplayName = "[Mail] 아이템 보상 수령 성공")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case03()
    {
        // Given
        const long userId = 3, mailId = 300, itemCode = 10002;
        var mail = new UserMail { mail_id = mailId, user_id = userId, reward_code = itemCode, count = 1 };

        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync(mail);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.InsertItemAsync(userId, It.Is<UserInventoryItem>(x => x.item_code == itemCode && x.level == 1)))
             .ReturnsAsync(true);
        _game.Setup(g => g.ReceiveCompleteMailAsync(mailId)).ReturnsAsync(true);

        // 트랜잭션 델리게이트 실제 실행
        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        _game.Verify(g => g.InsertItemAsync(userId, It.IsAny<UserInventoryItem>()), Times.Once);
        _game.Verify(g => g.ReceiveCompleteMailAsync(mailId), Times.Once);
    }

    /*
     * Target   : ReceiveMailAsync
     * Scenario : 룬 보상 수령 성공
     * Given    : reward_code = 룬(20000대), InsertRuneAsync == true, ReceiveCompleteMailAsync == true
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = None
     */
    [Fact(DisplayName = "[Mail] 룬 보상 수령 성공")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case04()
    {
        // Given
        const long userId = 4, mailId = 400, runeCode = 20001; // 룬 영역(20000대)
        var mail = new UserMail { mail_id = mailId, user_id = userId, reward_code = runeCode, count = 1 };

        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync(mail);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.InsertRuneAsync(userId, It.Is<UserInventoryRune>(x => x.rune_code == runeCode && x.level == 1)))
             .ReturnsAsync(true);
        _game.Setup(g => g.ReceiveCompleteMailAsync(mailId)).ReturnsAsync(true);

        // 트랜잭션 델리게이트 실제 실행
        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        _game.Verify(g => g.InsertRuneAsync(userId, It.IsAny<UserInventoryRune>()), Times.Once);
        _game.Verify(g => g.ReceiveCompleteMailAsync(mailId), Times.Once);
    }

    /*
     * Target   : ReceiveMailAsync
     * Scenario : 보상 지급은 성공했으나 메일 완료 처리 실패
     * Given    : 아이템 보상 성공, ReceiveCompleteMailAsync == false
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = FailedReceiveMail, 완료 처리 1회 시도됨
     */
    [Fact(DisplayName = "[Mail] 완료처리 실패 → 수령 실패")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case05()
    {
        // Given
        const long userId = 5, mailId = 500, itemCode = 10003;
        var mail = new UserMail { mail_id = mailId, user_id = userId, reward_code = itemCode, count = 1 };

        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync(mail);
        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.InsertItemAsync(userId, It.IsAny<UserInventoryItem>())).ReturnsAsync(true);
        _game.Setup(g => g.ReceiveCompleteMailAsync(mailId)).ReturnsAsync(false);

        // 트랜잭션 델리게이트 실제 실행
        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedReceiveMail);
        _game.Verify(g => g.InsertItemAsync(userId, It.IsAny<UserInventoryItem>()), Times.Once);
        _game.Verify(g => g.ReceiveCompleteMailAsync(mailId), Times.Once);
    }

    /*
     * Target   : ReceiveMailAsync
     * Scenario : 알 수 없는 보상 코드 → 실패 (완료 처리 호출 안됨)
     * Given    : reward_code = 9999 (0/1/2/10000~/20000~ 모두 아님)
     * When     : ReceiveMailAsync(userId, mailId)
     * Then     : result = FailedReceiveMail, ReceiveCompleteMailAsync 호출 안됨
     */
    [Fact(DisplayName = "[Mail] 알 수 없는 보상코드 → 수령 실패")]
    [Trait("Target", "ReceiveMailAsync")]
    public async Task ReceiveMailAsync_Case06()
    {
        // Given
        const long userId = 6, mailId = 600;
        var mail = new UserMail { mail_id = mailId, user_id = userId, reward_code = 9999, count = 1 }; // NONE
        _game.Setup(g => g.GetMailAsync(userId, mailId)).ReturnsAsync(mail);

        _memory.Setup(m => m.DeleteCacheData(userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        // 트랜잭션 델리게이트 실제 실행
        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.ReceiveMailAsync(userId, mailId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedReceiveMail);
        _game.Verify(g => g.ReceiveCompleteMailAsync(It.IsAny<long>()), Times.Never);
    }
}
