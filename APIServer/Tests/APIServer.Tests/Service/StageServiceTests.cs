using System.Collections.Immutable;
using APIServer;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using APIServer.Service;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;
using Xunit;

namespace APIServer.Tests.Service;

public class StageServiceTests
{
    private readonly Mock<IGameDb> _game = new();
    private readonly Mock<IMemoryDb> _memory = new();
    private readonly Mock<IMasterDb> _master = new();
    private readonly Mock<IQuestService> _quest = new();
    private readonly Mock<ILogger<StageService>> _logger = new();

    private StageService Sut() => new(_logger.Object, _game.Object, _memory.Object, _master.Object, _quest.Object);

    private static ImmutableDictionary<long, List<StageMonsterInfo>> Monsters(long stageCode, params (long code, int count)[] m)
        => ImmutableDictionary<long, List<StageMonsterInfo>>.Empty
            .Add(stageCode, m.Select(x => new StageMonsterInfo { stage_code = stageCode, monster_code = x.code, monster_count = x.count }).ToList());

    private static ImmutableDictionary<long, StageRewardGold> RewardGold(long stageCode, int gold)
        => ImmutableDictionary<long, StageRewardGold>.Empty.Add(stageCode, new StageRewardGold { stage_code = stageCode, gold = gold });

    private static ImmutableDictionary<long, List<StageRewardItem>> RewardItems(long stageCode, params (int code, int level, int drop)[] items)
        => ImmutableDictionary<long, List<StageRewardItem>>.Empty
            .Add(stageCode, items.Select(x => new StageRewardItem { stage_code = stageCode, item_code = x.code, level = x.level, drop_rate = x.drop }).ToList());

    private static ImmutableDictionary<long, List<StageRewardRune>> RewardRunes(long stageCode, params (int code, int level, int drop)[] runes)
        => ImmutableDictionary<long, List<StageRewardRune>>.Empty
            .Add(stageCode, runes.Select(x => new StageRewardRune { stage_code = stageCode, rune_code = x.code, drop_rate = x.drop }).ToList());

    private static InStageInfo MakeStage(long userId, string email, long stage, Dictionary<long,int> targets, Dictionary<long,int>? kills = null)
        => new()
        {
            userId = userId,
            email = email,
            stageCode = stage,
            startTime = DateTime.UtcNow,
            monsterKillTargets = targets,
            monsterKills = kills ?? targets.ToDictionary(kv => kv.Key, kv => 0)
        };

    // ----------------------------
    // EnterStage
    // ----------------------------

    /*
     * Target   : EnterStage
     * Scenario : 정상 진입 (캐싱 성공)
     * Given    : GetStageMonsterList()[stage] OK, CacheStageInfo == true
     * When     : EnterStage(userId, email, stage, chars)
     * Then     : None, 반환 리스트 개수 = 몬스터 종류 수
     */
    [Fact(DisplayName = "[Stage] 입장 성공 → 몬스터 리스트 반환")]
    [Trait("Target", "EnterStage")]
    public async Task EnterStage_Case01()
    {
        const long userId = 1, stage = 10L;
        var email = "u@test.com";
        _master.Setup(m => m.GetStageMonsterList()).Returns(Monsters(stage, (101, 3), (102, 2)));
        _memory.Setup(mm => mm.CacheStageInfo(It.IsAny<InStageInfo>())).ReturnsAsync(true);

        var sut = Sut();
        var result = await sut.EnterStage(userId, email, stage, new List<long> { 1, 2, 3 });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        _memory.Verify(mm => mm.CacheStageInfo(It.Is<InStageInfo>(s => s.userId == userId && s.stageCode == stage)), Times.Once);
    }

    /*
     * Target   : EnterStage
     * Scenario : 캐싱 실패
     * Given    : CacheStageInfo == false
     * When     : EnterStage
     * Then     : FailedCacheStageInfo
     */
    [Fact(DisplayName = "[Stage] 입장 실패(캐시 실패) → FailedCacheStageInfo")]
    [Trait("Target", "EnterStage")]
    public async Task EnterStage_Case02()
    {
        const long userId = 2, stage = 20L;
        _master.Setup(m => m.GetStageMonsterList()).Returns(Monsters(stage, (201, 1)));
        _memory.Setup(mm => mm.CacheStageInfo(It.IsAny<InStageInfo>())).ReturnsAsync(false);

        var sut = Sut();
        var result = await sut.EnterStage(userId, "x@y.com", stage, new List<long>());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedCacheStageInfo);
    }

    // ----------------------------
    // KillMonster
    // ----------------------------

    /*
     * Target   : KillMonster
     * Scenario : 인게임 정보 조회 실패
     * Given    : GetGameInfo → Failure(FailedLoadStageInfo)
     * When     : KillMonster
     * Then     : FailedLoadStageInfo
     */
    [Fact(DisplayName = "[Stage] 킬 실패(스테이지 정보 없음) → FailedLoadStageInfo")]
    [Trait("Target", "KillMonster")]
    public async Task KillMonster_Case01()
    {
        _memory.Setup(mm => mm.GetGameInfo(3)).ReturnsAsync(Result<InStageInfo>.Failure(ErrorCode.FailedLoadStageInfo));
        var sut = Sut();

        var result = await sut.KillMonster(3, 999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedLoadStageInfo);
    }

    /*
     * Target   : KillMonster
     * Scenario : 존재하지 않는 몬스터 코드
     * Given    : targets={101:3}, 요청=999
     * When     : KillMonster
     * Then     : CannotFindMonsterCode
     */
    [Fact(DisplayName = "[Stage] 킬 실패(없는 몬스터) → CannotFindMonsterCode")]
    [Trait("Target", "KillMonster")]
    public async Task KillMonster_Case02()
    {
        var stage = MakeStage(4, "a@b.com", 40, new() { { 101, 3 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));

        var sut = Sut();
        var result = await sut.KillMonster(stage.userId, 999);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotFindMonsterCode);
        _memory.Verify(mm => mm.CacheStageInfo(It.IsAny<InStageInfo>()), Times.Never);
    }

    /*
     * Target   : KillMonster
     * Scenario : 이미 목표 수만큼 처치
     * Given    : targets={101:1}, kills={101:1}
     * When     : KillMonster(101)
     * Then     : CannotKillMonster
     */
    [Fact(DisplayName = "[Stage] 킬 실패(이미 목표 달성) → CannotKillMonster")]
    [Trait("Target", "KillMonster")]
    public async Task KillMonster_Case03()
    {
        var stage = MakeStage(5, "c@d.com", 50, new() { { 101, 1 } }, new() { { 101, 1 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));

        var sut = Sut();
        var result = await sut.KillMonster(stage.userId, 101);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.CannotKillMonster);
        _memory.Verify(mm => mm.CacheStageInfo(It.IsAny<InStageInfo>()), Times.Never);
    }

    /*
     * Target   : KillMonster
     * Scenario : 정상 처치 → 캐시 갱신
     * Given    : targets={101:3}, kills={101:0}
     * When     : KillMonster(101)
     * Then     : None, CacheStageInfo 1회, kills[101]==1로 저장
     */
    [Fact(DisplayName = "[Stage] 킬 성공 → 캐시 갱신")]
    [Trait("Target", "KillMonster")]
    public async Task KillMonster_Case04()
    {
        var stage = MakeStage(6, "e@f.com", 60, new() { { 101, 3 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));
        _memory.Setup(mm => mm.CacheStageInfo(It.IsAny<InStageInfo>())).ReturnsAsync(true);

        var sut = Sut();
        var result = await sut.KillMonster(stage.userId, 101);

        result.IsSuccess.Should().BeTrue();
        _memory.Verify(mm => mm.CacheStageInfo(It.Is<InStageInfo>(s => s.monsterKills[101] == 1)), Times.Once);
    }

    // ----------------------------
    // ClearStage(userId, stageCode, clearFlag)
    // ----------------------------

    /*
     * Target   : ClearStage
     * Scenario : clearFlag=false → 인게임 정보만 삭제
     * Given    : GetGameInfo OK, DeleteStageInfo==true
     * When     : ClearStage(user, stage, false)
     * Then     : None, DeleteStageInfo 1회
     */
    [Fact(DisplayName = "[Stage] 종료 처리(클리어 아님) → 캐시 삭제 후 성공")]
    [Trait("Target", "ClearStage")]
    public async Task ClearStage_Case01()
    {
        var stage = MakeStage(7, "g@h.com", 70, new() { { 101, 1 } }, new() { { 101, 0 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));
        _memory.Setup(mm => mm.DeleteStageInfo(stage)).ReturnsAsync(true);

        var sut = Sut();
        var result = await sut.ClearStage(stage.userId, stage.stageCode, clearFlag: false);

        result.IsSuccess.Should().BeTrue();
        _memory.Verify(mm => mm.DeleteStageInfo(stage), Times.Once);
    }

    /*
     * Target   : ClearStage
     * Scenario : clearFlag=true 인데 아직 미클리어
     * Given    : targets != kills
     * When     : ClearStage(user, stage, true)
     * Then     : StageInProgress, DeleteStageInfo 호출 안됨
     */
    [Fact(DisplayName = "[Stage] 클리어 시도(미완) → StageInProgress")]
    [Trait("Target", "ClearStage")]
    public async Task ClearStage_Case02()
    {
        var stage = MakeStage(8, "i@j.com", 80, new() { { 101, 2 } }, new() { { 101, 1 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));

        var sut = Sut();
        var result = await sut.ClearStage(stage.userId, stage.stageCode, clearFlag: true);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.StageInProgress);
        _memory.Verify(mm => mm.DeleteStageInfo(It.IsAny<InStageInfo>()), Times.Never);
    }

    /*
     * Target   : ClearStage
     * Scenario : clearFlag=true, 스테이지 완료 → 업데이트 단계 실패
     * Given    : UpdateStageAsync == false (FindClearStageAsync OK)
     * When     : ClearStage(user, stage, true)
     * Then     : FailedUpdateClearStage, DeleteStageInfo 호출 안됨
     */
    [Fact(DisplayName = "[Stage] 클리어 업데이트 실패 → FailedUpdateClearStage")]
    [Trait("Target", "ClearStage")]
    public async Task ClearStage_Case03()
    {
        var stage = MakeStage(9, "k@l.com", 90, new() { { 101, 1 } }, new() { { 101, 1 } });
        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));

        _game.Setup(g => g.FindClearStageAsync(stage.userId, stage.stageCode))
             .ReturnsAsync(new UserClearStage { user_id = stage.userId, stage_code = (int)stage.stageCode, clear_count = 0 });
        _game.Setup(g => g.UpdateStageAsync(It.IsAny<UserClearStage>())).ReturnsAsync(false);

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        var sut = Sut();
        var result = await sut.ClearStage(stage.userId, stage.stageCode, true);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.FailedUpdateClearStage);
        _memory.Verify(mm => mm.DeleteStageInfo(It.IsAny<InStageInfo>()), Times.Never);
    }

    /*
     * Target   : ClearStage
     * Scenario : clearFlag=true, 스테이지 완료 → 전체 성공 경로
     * Given    : UpdateStageAsync true, 보상(골드/아이템/룬) 정상, 퀘스트 갱신 정상, 캐시 삭제 정상
     * When     : ClearStage(user, stage, true)
     * Then     : None, DeleteStageInfo 1회
     */
    [Fact(DisplayName = "[Stage] 클리어 성공 → 보상/캐시/퀘스트 처리 후 성공")]
    [Trait("Target", "ClearStage")]
    public async Task ClearStage_Case04()
    {
        const long stageCode = 100;
        var stage = MakeStage(10, "m@n.com", stageCode, new() { { 201, 1 } }, new() { { 201, 1 } });

        _memory.Setup(mm => mm.GetGameInfo(stage.userId)).ReturnsAsync(Result<InStageInfo>.Success(stage));

        _game.Setup(g => g.FindClearStageAsync(stage.userId, stage.stageCode))
             .ReturnsAsync(new UserClearStage { user_id = stage.userId, stage_code = (int)stage.stageCode, clear_count = 0 });
        _game.Setup(g => g.UpdateStageAsync(It.IsAny<UserClearStage>())).ReturnsAsync(true);

        _master.Setup(m => m.GetStageRewardsGold()).Returns(RewardGold(stageCode, gold: 50));
        _master.Setup(m => m.GetStageRewardsItem()).Returns(RewardItems(stageCode, (code: 10001, level: 1, drop: 100)));
        _master.Setup(m => m.GetStageRewardsRune()).Returns(RewardRunes(stageCode, (code: 20001, level: 1, drop: 100)));

        _game.Setup(g => g.GetUserDataByEmailAsync(stage.email))
             .ReturnsAsync(new UserGameData { user_id = stage.userId, gold = 0, gem = 0, exp = 0, level = 1 });

        _game.Setup(g => g.UpdateUserGoldAsync(stage.userId, It.IsAny<int>())).ReturnsAsync(true);
        _game.Setup(g => g.InsertDropItems(stage.userId, It.IsAny<List<StageRewardItem>>())).ReturnsAsync(true);
        _game.Setup(g => g.InsertDropRunes(stage.userId, It.IsAny<List<StageRewardRune>>())).ReturnsAsync(true);

        // UpdateClearStageAsync 내부 퀘스트 (KillMonster / ClearStage)
        _quest.Setup(q => q.RefreshQuestProgress(stage.userId, QuestType.KillMonster, It.IsAny<int>()))
              .ReturnsAsync(Result.Success());
        _quest.Setup(q => q.RefreshQuestProgress(stage.userId, QuestType.ClearStage, It.IsAny<int>()))
              .ReturnsAsync(Result.Success());

        // RewardClearStageAsync 내부 퀘스트 (GetGold / GetItem)
        _quest.Setup(q => q.RefreshQuestProgress(stage.userId, QuestType.GetGold, It.IsAny<int>()))
              .ReturnsAsync(Result.Success());
        _quest.Setup(q => q.RefreshQuestProgress(stage.userId, QuestType.GetItem, It.IsAny<int>()))
              .ReturnsAsync(Result.Success());

        _memory.Setup(mm => mm.DeleteCacheData(stage.userId, It.IsAny<List<CacheType>>()))
               .ReturnsAsync(Result.Success());

        _game.Setup(g => g.WithTransactionAsync(It.IsAny<Func<QueryFactory, Task<ErrorCode>>>()))
             .Returns((Func<QueryFactory, Task<ErrorCode>> f) => f(null!));

        _memory.Setup(mm => mm.DeleteStageInfo(stage)).ReturnsAsync(true);

        var sut = Sut();
        var result = await sut.ClearStage(stage.userId, stage.stageCode, true);

        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCode.None);

        _memory.Verify(mm => mm.DeleteStageInfo(stage), Times.Once);
        _game.Verify(g => g.UpdateStageAsync(It.IsAny<UserClearStage>()), Times.Once);
        _game.Verify(g => g.UpdateUserGoldAsync(stage.userId, It.IsAny<int>()), Times.Once);
        _game.Verify(g => g.InsertDropItems(stage.userId, It.IsAny<List<StageRewardItem>>()), Times.Once);
        _game.Verify(g => g.InsertDropRunes(stage.userId, It.IsAny<List<StageRewardRune>>()), Times.Once);
        _quest.Verify(q => q.RefreshQuestProgress(stage.userId, QuestType.KillMonster, It.IsAny<int>()), Times.Once);
        _quest.Verify(q => q.RefreshQuestProgress(stage.userId, QuestType.ClearStage, It.IsAny<int>()), Times.Once);
        _quest.Verify(q => q.RefreshQuestProgress(stage.userId, QuestType.GetGold, It.IsAny<int>()), Times.Once);
        _quest.Verify(q => q.RefreshQuestProgress(stage.userId, QuestType.GetItem, It.IsAny<int>()), Times.Once);
    }
}
