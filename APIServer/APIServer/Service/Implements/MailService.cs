using APIServer.Models.DTO;
using APIServer.Models.DTO.Mail;
using APIServer.Models.Entity;
using APIServer.Repository;
using APIServer.Repository.Implements.Memory;
using APIServer.Service.Implements;
using ZLogger;
using static APIServer.LoggerManager;

namespace APIServer.Service;

public class MailService (ILogger<MailService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    : IMailService
{
    private readonly ILogger<MailService> _logger = logger;
    private readonly IGameDb _gameDb = gameDb;
    private readonly IMemoryDb _memoryDb = memoryDb;
    
    // 아이템 식별 구분자, 로직을 간단하게 구현하기 위해서 Reward 종류를 구분하였다
    // Code 0 : Gold
    // Code 1 : Gem
    // Code 2 : Exp
    // Code 10000 ~ 19999 : Item
    // Code 20000 ~ 29999 : Rune
    private RewardType ConvertRewardType(long code)
    {
        if(code == 0) return RewardType.GOLD;
        if(code == 1) return RewardType.GEM;
        if(code == 2) return RewardType.EXP;
        if(code >= 20000) return RewardType.ITEM;
        if(code >= 10000) return RewardType.RUNE;
        return RewardType.NONE;
    }
    
    public async Task<Result> SendRewardMail(long userId, string title, long code, int price)
    {
        var newMail = new UserMail
        {
            mail_title = title,
            user_id = userId,
            earn_reward = false,
            send_date = DateTime.UtcNow,
            expire_date = DateTime.UtcNow.AddDays(30),
            reward_code = code,
            count = price,
        };

        if (await _gameDb.InsertNewMail(newMail) == false)
        {
            return Result.Failure(ErrorCode.FailedSendMail);
        }
        
        return Result.Success();
    }

    public async Task<Result<List<MailData>>> GetMailAsync(long userId, Pageable pageable)
    {
        try
        {
            var pagingMails = await _gameDb.GetUnReceiveMailByPaging(userId, pageable);
            var dto = pagingMails.Select(mail => new MailData
            {
                mailId = mail.mail_id,
                expireDate = mail.expire_date,
                title = mail.mail_title,
                rewardCode = mail.reward_code,
                rewardCount = mail.count,
                sendDate = mail.send_date,
            }).ToList();

            LogInfo(_logger, EventType.GetMail, "Get Mail", new { userId, pageable });
            
            return Result<List<MailData>>.Success(dto);
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedGetMail, EventType.GetMail, 
                "Failed Get Mail", new { userId, pageable, ex.Message, ex.StackTrace });
            return Result<List<MailData>>.Failure(ErrorCode.FailedGetMail);
        }
    }

    public async Task<Result> ReceiveMailAsync(long userId, long mailId)
    {
        try
        {
            // 메일 조회
            var mail = await _gameDb.GetMailAsync(userId, mailId);
            if (mail is null)
            {
                return Result.Failure(ErrorCode.CannotFindMail);
            }

            var txErrorCode = await _gameDb.WithTransactionAsync(async _ =>
            {
                // 메일 보상 획득
                if (await ReceiveReward(userId, mail.reward_code, mail.count))
                {
                    return ErrorCode.FailedReceiveMail;
                }

                // 메일 보상 획득 처리
                if (await _gameDb.ReceiveCompleteMailAsync(mailId) == false)
                {
                    return ErrorCode.FailedReceiveMail;
                }

                return ErrorCode.None;
            });

            if (txErrorCode != ErrorCode.None)
            {
                return Result.Failure(txErrorCode);
            }
            
            LogInfo(_logger, EventType.ReceiveMail, "Receive Mail", new { userId, mailId });

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(_logger, ErrorCode.FailedReceiveMail ,EventType.ReceiveMail, 
                "Failed Receive Mail", new { userId, mailId, ex.Message, ex.StackTrace });
            return Result.Failure(ErrorCode.FailedReceiveMail);;
        }
    }

    private async Task<bool> ReceiveReward(long userId, long code, int price)
    {
        var type = ConvertRewardType(code);
        if (type == RewardType.NONE) return false;

        // 보상 획득 전 캐시정보 삭제
        if (await _memoryDb.DeleteCacheData(userId, [CacheType.Item, CacheType.Rune, CacheType.UserGameData]) is var deleteCache && deleteCache.IsFailed)
        {
            return false;
        }
        
        if (type == RewardType.ITEM)
        {
            return await _gameDb.InsertItemAsync(userId, new UserInventoryItem { item_code = code, level = 1 });
        }

        if (type == RewardType.RUNE)
        {
            return await _gameDb.InsertRuneAsync(userId, new UserInventoryRune { rune_code = code, level = 1 });
        }
        
        var userData = await _gameDb.GetUserDataByUserIdAsync(userId);
        if (type == RewardType.GOLD)
        {
            var newGold = userData.gold + price;
            return await _gameDb.UpdateUserGoldAsync(userId, newGold);
        }

        if (type == RewardType.GEM)
        {
            var newGem = userData.gem + price;
            return await _gameDb.UpdateUserGemAsync(userId, newGem);
        }

        if (type == RewardType.EXP)
        {
            var (newLevel, newExp) = (userData.level, userData.exp + price);
            if (newExp >= 100)
            {
                newLevel += newExp / 100;
                newExp %= 100;
                return await _gameDb.UpdateUserExpAsync(userId, newExp, newLevel);
            }
        }
        return false;
    }
}

public enum RewardType
{
    NONE = -1,
    GOLD = 0,
    GEM,
    EXP,
    ITEM,
    RUNE,
}