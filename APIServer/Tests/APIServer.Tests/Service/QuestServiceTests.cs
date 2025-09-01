using System.Collections.Immutable;
using APIServer;
using APIServer.Models.DTO;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Repository;
using APIServer.Service;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;
using Xunit;

namespace APIServer.Tests.Service;

public class QuestServiceTests
{
    private readonly Mock<IGameDb> _gameDb = new();
    private readonly Mock<IMasterDb> _masterDb = new();
    private readonly Mock<IMemoryDb> _memoryDb = new();
    private readonly Mock<ILogger<QuestService>> _logger = new();

    private QuestService Sut()
        => new(_logger.Object, _gameDb.Object, _masterDb.Object, _memoryDb.Object);

    private static ImmutableDictionary<long, QuestInfoData> Map(params QuestInfoData[] items)
        => items.ToDictionary(x => x.quest_code, x => x).ToImmutableDictionary();

    // ------------------------------------------------------------
    // RefreshQuestProgress
    // ------------------------------------------------------------

    /*
     * Target   : RefreshQuestProgress
     * Scenario : 대상 타입의 진행 중 퀘스트가 없음
     * Given    : GetProgressQuestByType() => []
     * When     : RefreshQuestProgress(user, type, add)
     * Then     : 성공, CompleteQuest 호출 안 됨, 캐시 삭제 1회
     */
    [Fact(DisplayName = "[Quest] 진행중 퀘스트 없음 → 성공 & 완료처리 없음")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case01()
    {
        // Given
        const long userId = 1;
        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.GetGold))
               .ReturnsAsync(new List<UserQuestInprogress>());
        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map()); // 접근 안되지만 방어
        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.GetGold, addValue: 5);

        // Then
        result.IsSuccess.Should().BeTrue();
        _gameDb.Verify(g => g.CompleteQuest(It.IsAny<long>(), It.IsAny<List<long>>()), Times.Never);
        _memoryDb.Verify(m => m.DeleteCachedQuestList(userId), Times.Once);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : 누적형(≠ClearStage) 진행도가 목표 미달
     * Given    : progress 10, target 100, add 5 → 15(<100)
     * When     : RefreshQuestProgress
     * Then     : 성공, CompleteQuest 호출 안 됨, 캐시 삭제 1회
     */
    [Fact(DisplayName = "[Quest] 누적형 미완료 → 완료처리 없이 성공")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case02()
    {
        // Given
        const long userId = 10;
        const long questCode = 1001;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.GetGold))
               .ReturnsAsync(new List<UserQuestInprogress>
               {
                   new() { quest_inprogress_id = 999, quest_code = questCode, progress = 10 }
               });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetGold,
                     quest_progress = 100
                 }));

        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.GetGold, addValue: 5);

        // Then
        result.IsSuccess.Should().BeTrue();
        _gameDb.Verify(g => g.CompleteQuest(It.IsAny<long>(), It.IsAny<List<long>>()), Times.Never);
        _memoryDb.Verify(m => m.DeleteCachedQuestList(userId), Times.Once);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : 누적형(≠ClearStage) 목표 도달(>=)
     * Given    : progress 90, target 100, add 10 → 100(완료)
     * When     : RefreshQuestProgress
     * Then     : 성공, CompleteQuest(quest_code 포함) 1회, 캐시 삭제 1회
     */
    [Fact(DisplayName = "[Quest] 누적형 목표 도달 → 완료처리 성공")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case03()
    {
        // Given
        const long userId = 11;
        const long questCode = 2001;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.GetExp))
               .ReturnsAsync(new List<UserQuestInprogress>
               {
                   new() { quest_inprogress_id = 555, quest_code = questCode, progress = 90 }
               });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetExp,
                     quest_progress = 100
                 }));

        _gameDb.Setup(g => g.CompleteQuest(userId, It.Is<List<long>>(l => l.Count == 1 && l.Contains(questCode))))
               .ReturnsAsync(true);

        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.GetExp, addValue: 10);

        // Then
        result.IsSuccess.Should().BeTrue();
        _gameDb.Verify(g => g.CompleteQuest(userId, It.Is<List<long>>(l => l.Count == 1 && l.Contains(questCode))), Times.Once);
        _memoryDb.Verify(m => m.DeleteCachedQuestList(userId), Times.Once);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : ClearStage 타입에서 addValue != target → 미완료
     * Given    : target.quest_progress = 1, addValue = 0
     * When     : RefreshQuestProgress
     * Then     : 성공, CompleteQuest 호출 안 됨
     */
    [Fact(DisplayName = "[Quest] ClearStage: add != target → 미완료")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case04()
    {
        // Given
        const long userId = 12;
        const long questCode = 3001;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.ClearStage))
               .ReturnsAsync(new List<UserQuestInprogress>
               {
                   new() { quest_inprogress_id = 1, quest_code = questCode, progress = 0 }
               });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.ClearStage,
                     quest_progress = 1
                 }));

        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.ClearStage, addValue: 0);

        // Then
        result.IsSuccess.Should().BeTrue();
        _gameDb.Verify(g => g.CompleteQuest(It.IsAny<long>(), It.IsAny<List<long>>()), Times.Never);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : ClearStage 타입에서 addValue == target → 완료
     * Given    : target.quest_progress = 1, addValue = 1
     * When     : RefreshQuestProgress
     * Then     : 성공, CompleteQuest(quest_code 포함) 1회
     */
    [Fact(DisplayName = "[Quest] ClearStage: add == target → 완료 처리")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case05()
    {
        // Given
        const long userId = 13;
        const long questCode = 3002;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.ClearStage))
               .ReturnsAsync(new List<UserQuestInprogress>
               {
                   new() { quest_inprogress_id = 2, quest_code = questCode, progress = 0 }
               });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.ClearStage,
                     quest_progress = 1
                 }));

        _gameDb.Setup(g => g.CompleteQuest(userId, It.Is<List<long>>(l => l.Count == 1 && l.Contains(questCode))))
               .ReturnsAsync(true);

        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Success());

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.ClearStage, addValue: 1);

        // Then
        result.IsSuccess.Should().BeTrue();
        _gameDb.Verify(g => g.CompleteQuest(userId, It.Is<List<long>>(l => l.Count == 1 && l.Contains(questCode))), Times.Once);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : 완료 리스트 존재하나 DB 완료 처리 실패
     * Given    : CompleteQuest(...) == false
     * When     : RefreshQuestProgress
     * Then     : FailedCompleteQuest 반환
     */
    [Fact(DisplayName = "[Quest] 완료 처리 실패 → FailedCompleteQuest")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case06()
    {
        // Given
        const long userId = 14;
        const long questCode = 4001;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.GetItem))
               .ReturnsAsync(new List<UserQuestInprogress>
               {
                   new() { quest_inprogress_id = 3, quest_code = questCode, progress = 5 }
               });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetItem,
                     quest_progress = 10
                 }));

        _gameDb.Setup(g => g.CompleteQuest(userId, It.Is<List<long>>(l => l.Contains(questCode))))
               .ReturnsAsync(false);

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.GetItem, addValue: 5);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCompleteQuest);
    }

    /*
     * Target   : RefreshQuestProgress
     * Scenario : 캐시 삭제 실패
     * Given    : DeleteCachedQuestList → Failure(FailedCacheGameData)
     * When     : RefreshQuestProgress
     * Then     : FailedCacheGameData 반환
     */
    [Fact(DisplayName = "[Quest] 캐시 삭제 실패 → FailedCacheGameData")]
    [Trait("Target", "RefreshQuestProgress")]
    public async Task RefreshQuestProgress_Case07()
    {
        // Given
        const long userId = 15;

        _gameDb.Setup(g => g.GetProgressQuestByType(userId, QuestType.KillMonster))
               .ReturnsAsync(new List<UserQuestInprogress>()); // 완료 없음
        _masterDb.Setup(m => m.GetQuestInfoDatas()).Returns(Map());
        _memoryDb.Setup(m => m.DeleteCachedQuestList(userId))
                 .ReturnsAsync(Result.Failure(ErrorCode.FailedCacheGameData));

        var sut = Sut();

        // When
        var result = await sut.RefreshQuestProgress(userId, QuestType.KillMonster, addValue: 1);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheGameData);
    }
    /*
     * Target   : RewardQuest
     * Scenario : 이미 보상 수령한 퀘스트
     * Given    : GetCompleteQuest(...).earn_reward = true
     * When     : RewardQuest(userId, questCode)
     * Then     : AlreadyEarnReward 반환, 내부 처리 없음
     */
    [Fact(DisplayName = "[Quest] 이미 보상 수령 → AlreadyEarnReward")]
    [Trait("Target", "RewardQuest")]
    public async Task RewardQuest_Case01()
    {
        // Given
        const long userId = 1;
        const long questCode = 1001;

        _gameDb.Setup(g => g.GetCompleteQuest(userId, questCode))
               .ReturnsAsync(new UserQuestComplete { quest_code = questCode, earn_reward = true });

        var sut = Sut();

        // When
        var result = await sut.RewardQuest(userId, questCode);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.AlreadyEarnReward);
        _gameDb.Verify(g => g.RewardCompleteQuest(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        _gameDb.Verify(g => g.UpdateUserCurrencyAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    /*
     * Target   : RewardQuest
     * Scenario : 보상 지급 단계 실패 (통화 업데이트 실패)
     * Given    : UpdateUserCurrencyAsync == false
     * When     : RewardQuest(userId, questCode)
     * Then     : FailedRewardQuest, RewardCompleteQuest 호출 안됨
     */
    [Fact(DisplayName = "[Quest] 보상 지급 실패 → FailedRewardQuest")]
    [Trait("Target", "RewardQuest")]
    public async Task RewardQuest_Case02()
    {
        // Given
        const long userId = 2;
        const long questCode = 2001;

        _gameDb.Setup(g => g.GetCompleteQuest(userId, questCode))
               .ReturnsAsync(new UserQuestComplete { quest_code = questCode, earn_reward = false });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetGold,
                     reward_gold = 10,
                     reward_gem = 0,
                     reward_exp = 0
                 }));

        _gameDb.Setup(g => g.GetUserDataByUserIdAsync(userId))
               .ReturnsAsync(new FullGameData { gold = 100, gem = 5, exp = 0, level = 1 });

        _gameDb.Setup(g => g.UpdateUserCurrencyAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
               .ReturnsAsync(false);

        _gameDb.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
               .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.RewardQuest(userId, questCode);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedRewardQuest);
        _gameDb.Verify(g => g.RewardCompleteQuest(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    /*
     * Target   : RewardQuest
     * Scenario : 보상 지급은 성공했으나 보상 수령 표시 실패
     * Given    : UpdateUserCurrencyAsync == true, RewardCompleteQuest == false
     * When     : RewardQuest(userId, questCode)
     * Then     : FailedRewardQuest
     */
    [Fact(DisplayName = "[Quest] 완료표시 실패 → FailedRewardQuest")]
    [Trait("Target", "RewardQuest")]
    public async Task RewardQuest_Case03()
    {
        // Given
        const long userId = 3;
        const long questCode = 3001;

        _gameDb.Setup(g => g.GetCompleteQuest(userId, questCode))
               .ReturnsAsync(new UserQuestComplete { quest_code = questCode, earn_reward = false });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetGold,
                     reward_gold = 0,
                     reward_gem = 5,
                     reward_exp = 0
                 }));

        _gameDb.Setup(g => g.GetUserDataByUserIdAsync(userId))
               .ReturnsAsync(new FullGameData { gold = 0, gem = 0, exp = 0, level = 1 });

        _gameDb.Setup(g => g.UpdateUserCurrencyAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
               .ReturnsAsync(true);

        _gameDb.Setup(g => g.RewardCompleteQuest(userId, questCode))
               .ReturnsAsync(false);

        _gameDb.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
               .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.RewardQuest(userId, questCode);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedRewardQuest);
        _gameDb.Verify(g => g.UpdateUserCurrencyAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _gameDb.Verify(g => g.RewardCompleteQuest(userId, questCode), Times.Once);
    }

    /*
     * Target   : RewardQuest
     * Scenario : 정상 보상 수령
     * Given    : UpdateUserCurrencyAsync == true, RewardCompleteQuest == true
     * When     : RewardQuest(userId, questCode)
     * Then     : None 반환
     */
    [Fact(DisplayName = "[Quest] 보상 수령 성공 → None")]
    [Trait("Target", "RewardQuest")]
    public async Task RewardQuest_Case04()
    {
        // Given
        const long userId = 4;
        const long questCode = 4001;

        _gameDb.Setup(g => g.GetCompleteQuest(userId, questCode))
               .ReturnsAsync(new UserQuestComplete { quest_code = questCode, earn_reward = false });

        _masterDb.Setup(m => m.GetQuestInfoDatas())
                 .Returns(Map(new QuestInfoData
                 {
                     quest_code = questCode,
                     quest_type = QuestType.GetExp,
                     reward_gold = 0,
                     reward_gem = 0,
                     reward_exp = 50
                 }));

        _gameDb.Setup(g => g.GetUserDataByUserIdAsync(userId))
               .ReturnsAsync(new FullGameData { gold = 10, gem = 1, exp = 60, level = 1 });

        _gameDb.Setup(g => g.UpdateUserCurrencyAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
               .ReturnsAsync(true);

        _gameDb.Setup(g => g.RewardCompleteQuest(userId, questCode))
               .ReturnsAsync(true);

        _gameDb.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
               .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();

        // When
        var result = await sut.RewardQuest(userId, questCode);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);
        _gameDb.Verify(g => g.RewardCompleteQuest(userId, questCode), Times.Once);
    }
}
