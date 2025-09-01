using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using APIServer;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Repository;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution; // QueryFactory
using Xunit;

namespace APIServer.Tests.Service;

public class AttendanceServiceTests
{
    private readonly Mock<IMasterDb> _masterDb = new();
    private readonly Mock<IGameDb> _gameDb = new();
    private readonly Mock<IMailService> _mail = new();
    private readonly Mock<ILogger<AttendanceService>> _logger = new();

    private AttendanceService Sut() =>
        new(_logger.Object, _masterDb.Object, _gameDb.Object, _mail.Object);

    // 트랜잭션 람다를 실제로 실행시키는 공통 Setup
    private void SetupTxPassthrough()
    {
        _gameDb
            .Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
            .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));
    }

    // 보상 테이블 헬퍼: day 키로 인덱싱 가능한 ImmutableDictionary 구성
    private static ImmutableDictionary<int, AttendanceRewardMonth> RewardMap(params (int day, long code, int count)[] items)
    {
        var b = ImmutableDictionary.CreateBuilder<int, AttendanceRewardMonth>();
        foreach (var (day, code, count) in items)
            b[day] = new AttendanceRewardMonth { day = day, item_code = code, count = count };
        return b.ToImmutable();
    }

    // =====================================================================
    // AttendanceAndRewardAsync
    // =====================================================================

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 이번 달 출석을 이미 모두 완료
     * Given    : last_attendance_date == DaysInMonth
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : AttendanceAlreadyComplete
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 월 출석 모두 완료 → AttendanceAlreadyComplete")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case01()
    {
        // Given
        const long userId = 1;
        var today = DateTime.Now;
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var att = new UserAttendanceMonth
        {
            last_attendance_date = daysInMonth,
            last_update_date = today.AddDays(-1),
            start_update_date = today.AddDays(-10)
        };
        _gameDb.Setup(g => g.GetUserAttendance(userId)).ReturnsAsync(att);

        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.AttendanceAlreadyComplete);
    }

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 오늘 이미 출석함
     * Given    : last_update_date.Date == DateTime.Now.Date
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : AttendanceAlreadyDoneToday
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 오늘 이미 출석 완료 → AttendanceAlreadyDoneToday")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case02()
    {
        // Given
        const long userId = 2;
        var att = new UserAttendanceMonth
        {
            last_attendance_date = 1,
            last_update_date = DateTime.Now, // 오늘
            start_update_date = DateTime.Now.AddDays(-3)
        };
        _gameDb.Setup(g => g.GetUserAttendance(userId)).ReturnsAsync(att);

        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.AttendanceAlreadyDoneToday);
    }

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 정상 흐름(마지막 날 아님) — 출석 + 보상 지급 성공
     * Given    : UpdateAttendanceToday == true, SendRewardMail == Success, 마지막날 아님
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : None, ResetAttendanceDay 호출 안 됨
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 정상(마지막 날 아님) → None")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case03()
    {
        // Given
        const long userId = 3;
        var att = new UserAttendanceMonth
        {
            last_attendance_date = 1, // +1 → day=2
            last_update_date = DateTime.Now.AddDays(-1),
            start_update_date = DateTime.Now.AddDays(-10)
        };

        _gameDb.Setup(g => g.GetUserAttendance(userId)).ReturnsAsync(att);
        _gameDb.Setup(g => g.UpdateAttendanceToday(userId, 2)).ReturnsAsync(true);

        _masterDb.Setup(m => m.GetAttendanceRewardMonths())
                 .Returns(RewardMap((2, 10001, 3)));

        _mail.Setup(m => m.SendRewardMail(userId, "출석 보상", 10001, 3))
             .ReturnsAsync(Result.Success());

        // 마지막 날 아님 → ResetAttendanceDay는 호출되지 않아야 함
        _gameDb.Setup(g => g.ResetAttendanceDay(It.IsAny<long>())).ReturnsAsync(true);

        SetupTxPassthrough();
        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        _gameDb.Verify(g => g.UpdateAttendanceToday(userId, 2), Times.Once);
        _mail.Verify(m => m.SendRewardMail(userId, "출석 보상", 10001, 3), Times.Once);
        _gameDb.Verify(g => g.ResetAttendanceDay(It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 보상 송신 실패
     * Given    : SendRewardMail == Failure(FailedAttendanceReward)
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : FailedAttendanceReward
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 보상 송신 실패 → FailedAttendanceReward")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case04()
    {
        // Given
        const long userId = 4;
        var att = new UserAttendanceMonth
        {
            last_attendance_date = 2, // +1 → day=3
            last_update_date = DateTime.Now.AddDays(-1),
            start_update_date = DateTime.Now.AddDays(-10)
        };

        _gameDb.Setup(g => g.GetUserAttendance(userId)).ReturnsAsync(att);
        _gameDb.Setup(g => g.UpdateAttendanceToday(userId, 3)).ReturnsAsync(true);

        _masterDb.Setup(m => m.GetAttendanceRewardMonths())
                 .Returns(RewardMap((3, 20000, 1)));

        _mail.Setup(m => m.SendRewardMail(userId, "출석 보상", 20000, 1))
             .ReturnsAsync(Result.Failure(ErrorCode.FailedAttendanceReward));

        SetupTxPassthrough();
        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedAttendanceReward);
    }

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 마지막 날 보상 후 리셋 실패
     * Given    : newDay == DaysInMonth, ResetAttendanceDay == false
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : FailedAttendanceReset
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 마지막 날 Reset 실패 → FailedAttendanceReset")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case05()
    {
        // Given
        const long userId = 5;
        var now = DateTime.Now;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        var att = new UserAttendanceMonth
        {
            last_attendance_date = daysInMonth - 1, // +1 → 마지막 날
            last_update_date = now.AddDays(-1),
            start_update_date = now.AddDays(-15)
        };

        _gameDb.Setup(g => g.GetUserAttendance(userId)).ReturnsAsync(att);
        _gameDb.Setup(g => g.UpdateAttendanceToday(userId, daysInMonth)).ReturnsAsync(true);

        _masterDb.Setup(m => m.GetAttendanceRewardMonths())
                 .Returns(RewardMap((daysInMonth, 30001, 1)));

        _mail.Setup(m => m.SendRewardMail(userId, "출석 보상", 30001, 1))
             .ReturnsAsync(Result.Success());

        _gameDb.Setup(g => g.ResetAttendanceDay(userId)).ReturnsAsync(false);

        SetupTxPassthrough();
        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedAttendanceReset);
        _gameDb.Verify(g => g.ResetAttendanceDay(userId), Times.Once);
    }

    /*
     * Target   : AttendanceAndRewardAsync
     * Scenario : 사전 조회 단계에서 예외 발생
     * Given    : GetUserAttendance throws
     * When     : AttendanceAndRewardAsync(userId)
     * Then     : FailedAttendance
     */
    [Fact(DisplayName = "[AttendanceAndRewardAsync] 예외 발생 → FailedAttendance")]
    [Trait("Target", "AttendanceAndRewardAsync")]
    public async Task AttendanceAndRewardAsync_Case06()
    {
        // Given
        const long userId = 6;
        _gameDb.Setup(g => g.GetUserAttendance(userId)).ThrowsAsync(new Exception("DB error"));

        var sut = Sut();

        // When
        var result = await sut.AttendanceAndRewardAsync(userId);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedAttendance);
    }
}
