using System;
using APIServer.Models.Entity;
using APIServer.Repository;
using APIServer.Service;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace APIServer.Tests.Service;

public class AccountServiceTests
{
    private readonly Mock<IAccountDb> _accountDb = new();
    private readonly Mock<IGameDb> _gameDb = new();
    private readonly Mock<IMemoryDb> _memoryDb = new();
    private readonly Mock<ISecurityService> _security = new();
    private readonly Mock<ILogger<AccountService>> _logger = new();

    private AccountService Sut()
        => new(_logger.Object, _accountDb.Object, _gameDb.Object, _memoryDb.Object, _security.Object);

    // =====================================================================
    // RegisterAccountAsync
    // =====================================================================

    /*
     * Target   : RegisterAccountAsync
     * Scenario : 이메일이 이미 존재함
     * Given    : CheckExistAccountByEmailAsync(email) == true
     * When     : RegisterAccountAsync(email, pw)
     * Then     : result = DuplicatedEmail, 추가 작업 호출 없음
     */
    [Fact(DisplayName = "[Register] 이메일 중복이면 DuplicatedEmail")]
    [Trait("Target", "RegisterAccountAsync")]
    public async Task RegisterAccountAsync_Case01()
    {
        // Given
        const string email = "dup@test.com";
        _accountDb.Setup(db => db.CheckExistAccountByEmailAsync(email)).ReturnsAsync(true);
        var sut = Sut();

        // When
        var result = await sut.RegisterAccountAsync(email, "pw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.DuplicatedEmail);

        _accountDb.Verify(x => x.CheckExistAccountByEmailAsync(email), Times.Once);
        _accountDb.VerifyNoOtherCalls();
        _gameDb.VerifyNoOtherCalls();
        _memoryDb.VerifyNoOtherCalls();
        _security.VerifyNoOtherCalls();
    }

    /*
     * Target   : RegisterAccountAsync
     * Scenario : 신규 이메일, 기본 게임데이터 생성 및 계정 생성 성공
     * Given    : 기본 데이터/HashPassword/계정 생성 모두 성공
     * When     : RegisterAccountAsync(email, pw)
     * Then     : result = None, HashPassword 호출 및 계정 생성 호출 검증
     */
    [Fact(DisplayName = "[Register] 신규 이메일이면 계정 생성 성공(None)")]
    [Trait("Target", "RegisterAccountAsync")]
    public async Task RegisterAccountAsync_Case02()
    {
        // Given
        const string email = "new@test.com";
        const string pw = "pw";
        const long userId = 42;
        const string expectedHash = "HASHED_PW";

        _accountDb.Setup(db => db.CheckExistAccountByEmailAsync(email)).ReturnsAsync(false);

        _gameDb.Setup(db => db.CreateUserGameDataAndReturnUserIdAsync()).ReturnsAsync(userId);
        _gameDb.Setup(db => db.InsertNewCharacterAsync(userId, It.IsAny<long>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertItemAsync(userId, It.IsAny<UserInventoryItem>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertRuneAsync(userId, It.IsAny<UserInventoryRune>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertAttendanceMonthAsync(userId)).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertQuestAsync(userId, It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(true);

        _security.Setup(s => s.HashPassword(pw, It.IsAny<string>()))
                 .Returns((true, expectedHash));

        _accountDb.Setup(db => db.CreateAccountUserDataAsync(
                            userId,
                            email,
                            It.Is<string>(salt => !string.IsNullOrEmpty(salt)),
                            expectedHash))
                  .ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.RegisterAccountAsync(email, pw);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);

        _security.Verify(s => s.HashPassword(pw, It.Is<string>(salt => !string.IsNullOrEmpty(salt))), Times.Once);
        _accountDb.Verify(x => x.CreateAccountUserDataAsync(
            userId, email, It.Is<string>(salt => !string.IsNullOrEmpty(salt)), expectedHash), Times.Once);
        _gameDb.Verify(x => x.DeleteGameDataByUserIdAsync(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : RegisterAccountAsync
     * Scenario : 계정 생성 실패 -> 롤백은 수행되지만 현재 구현은 None 반환
     * Given    : CreateAccountUserDataAsync == false
     * When     : RegisterAccountAsync(email, pw)
     * Then     : result = None, DeleteGameDataByUserIdAsync(userId) 1회 호출
     * NOTE     : 정책에 따라 FailedRegister 반환으로 바꿀 수 있음 (서비스/테스트 동시 수정)
     */
    [Fact(DisplayName = "[Register] 계정 생성 실패 시 롤백(현재 구현은 None 반환)")]
    [Trait("Target", "RegisterAccountAsync")]
    public async Task RegisterAccountAsync_Case03()
    {
        // Given
        const string email = "fail@test.com";
        const string pw = "pw";
        const long userId = 100;
        const string expectedHash = "HASHED";

        _accountDb.Setup(db => db.CheckExistAccountByEmailAsync(email)).ReturnsAsync(false);

        _gameDb.Setup(db => db.CreateUserGameDataAndReturnUserIdAsync()).ReturnsAsync(userId);
        _gameDb.Setup(db => db.InsertNewCharacterAsync(userId, It.IsAny<long>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertItemAsync(userId, It.IsAny<UserInventoryItem>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertRuneAsync(userId, It.IsAny<UserInventoryRune>())).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertAttendanceMonthAsync(userId)).ReturnsAsync(true);
        _gameDb.Setup(db => db.InsertQuestAsync(userId, It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(true);

        _security.Setup(s => s.HashPassword(pw, It.IsAny<string>()))
                 .Returns((true, expectedHash));

        _accountDb.Setup(db => db.CreateAccountUserDataAsync(
                            userId, email, It.Is<string>(salt => !string.IsNullOrEmpty(salt)), expectedHash))
                  .ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.RegisterAccountAsync(email, pw);

        // Then
        result.IsSuccess.Should().BeTrue(); // 현재 구현 기준
        result.ErrorCode.Should().Be(ErrorCode.None);
        _gameDb.Verify(x => x.DeleteGameDataByUserIdAsync(userId), Times.Once);
    }

    /*
     * Target   : RegisterAccountAsync
     * Scenario : 저장소 예외 발생
     * Given    : CheckExistAccountByEmailAsync 가 예외 throw
     * When     : RegisterAccountAsync(email, pw)
     * Then     : result = FailedRegister
     */
    [Fact(DisplayName = "[Register] 예외 발생 시 FailedRegister 반환")]
    [Trait("Target", "RegisterAccountAsync")]
    public async Task RegisterAccountAsync_Case04()
    {
        // Given
        _accountDb.Setup(db => db.CheckExistAccountByEmailAsync(It.IsAny<string>()))
                  .ThrowsAsync(new TimeoutException("DB timeout"));
        var sut = Sut();

        // When
        var result = await sut.RegisterAccountAsync("x@y.com", "pw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedRegister);
    }

    // =====================================================================
    // LoginAsync
    // =====================================================================

    /*
     * Target   : LoginAsync
     * Scenario : 정상 로그인 (검증 성공 & 세션 등록 성공)
     * Given    : VerifyPassword == true, RegisterSessionAsync == true
     * When     : LoginAsync(email, pw)
     * Then     : result = (userId, token), RegisterSessionAsync(UserSession) 1회
     */
    [Fact(DisplayName = "[Login] 정상 로그인 시 userId/토큰 반환")]
    [Trait("Target", "LoginAsync")]
    public async Task LoginAsync_Case01()
    {
        // Given
        const string email = "user@test.com";
        const string pw = "pw";
        const long userId = 77;
        const string token = "TOKEN123";

        var account = new UserAccount
        {
            account_id = 1,
            user_id = userId,
            email = email,
            password = "hashed",
            salt_value = "salt"
        };

        _accountDb.Setup(db => db.GetUserAccountByEmailAsync(email)).ReturnsAsync(account);
        _security.Setup(s => s.VerifyPassword(account.password, account.salt_value, pw)).Returns(true);
        _security.Setup(s => s.GenerateAuthToken()).Returns(token);
        _memoryDb.Setup(db => db.RegisterSessionAsync(It.IsAny<UserSession>())).ReturnsAsync(true);

        var sut = Sut();

        // When
        var result = await sut.LoginAsync(email, pw);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be((userId, token));
        _memoryDb.Verify(m => m.RegisterSessionAsync(It.Is<UserSession>(s =>
            s.userId == userId && s.email == email && s.authToken == token)), Times.Once);
    }

    /*
     * Target   : LoginAsync
     * Scenario : 계정이 없음
     * Given    : GetUserAccountByEmailAsync == null
     * When     : LoginAsync(email, pw)
     * Then     : result = CannotFindAccountUser
     */
    [Fact(DisplayName = "[Login] 계정 없으면 CannotFindAccountUser")]
    [Trait("Target", "LoginAsync")]
    public async Task LoginAsync_Case02()
    {
        // Given
        const string email = "noone@test.com";
        _accountDb.Setup(db => db.GetUserAccountByEmailAsync(email)).ReturnsAsync((UserAccount?)null);
        var sut = Sut();

        // When
        var result = await sut.LoginAsync(email, "pw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindAccountUser);
    }

    /*
     * Target   : LoginAsync
     * Scenario : 비밀번호 검증 실패
     * Given    : VerifyPassword == false
     * When     : LoginAsync(email, wrongPw)
     * Then     : result = FailedPasswordVerify
     */
    [Fact(DisplayName = "[Login] 비밀번호 틀리면 FailedPasswordVerify")]
    [Trait("Target", "LoginAsync")]
    public async Task LoginAsync_Case03()
    {
        // Given
        const string email = "user@test.com";
        var account = new UserAccount
        {
            account_id = 1,
            user_id = 50,
            email = email,
            password = "hashed",
            salt_value = "salt"
        };

        _accountDb.Setup(db => db.GetUserAccountByEmailAsync(email)).ReturnsAsync(account);
        _security.Setup(s => s.VerifyPassword(account.password, account.salt_value, "wrongpw")).Returns(false);

        var sut = Sut();

        // When
        var result = await sut.LoginAsync(email, "wrongpw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedPasswordVerify);
    }

    /*
     * Target   : LoginAsync
     * Scenario : 세션 등록 실패
     * Given    : VerifyPassword == true, RegisterSessionAsync == false
     * When     : LoginAsync(email, pw)
     * Then     : result = FailedRegisterSession
     */
    [Fact(DisplayName = "[Login] 세션 등록 실패 시 FailedRegisterSession")]
    [Trait("Target", "LoginAsync")]
    public async Task LoginAsync_Case04()
    {
        // Given
        const string email = "user@test.com";
        const long userId = 99;
        const string token = "TKN";

        var account = new UserAccount
        {
            account_id = 1,
            user_id = userId,
            email = email,
            password = "hashed",
            salt_value = "salt"
        };

        _accountDb.Setup(db => db.GetUserAccountByEmailAsync(email)).ReturnsAsync(account);
        _security.Setup(s => s.VerifyPassword(account.password, account.salt_value, "pw")).Returns(true);
        _security.Setup(s => s.GenerateAuthToken()).Returns(token);
        _memoryDb.Setup(db => db.RegisterSessionAsync(It.IsAny<UserSession>())).ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.LoginAsync(email, "pw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedRegisterSession);
    }

    /*
     * Target   : LoginAsync
     * Scenario : 저장소 예외 발생
     * Given    : GetUserAccountByEmailAsync 가 예외 throw
     * When     : LoginAsync(email, pw)
     * Then     : result = FailedLogin
     */
    [Fact(DisplayName = "[Login] 예외 발생 시 FailedLogin 반환")]
    [Trait("Target", "LoginAsync")]
    public async Task LoginAsync_Case05()
    {
        // Given
        _accountDb.Setup(db => db.GetUserAccountByEmailAsync(It.IsAny<string>()))
                  .ThrowsAsync(new Exception("DB error"));
        var sut = Sut();

        // When
        var result = await sut.LoginAsync("x@y.com", "pw");

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedLogin);
    }
}
