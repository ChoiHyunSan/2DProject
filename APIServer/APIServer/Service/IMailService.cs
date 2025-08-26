using APIServer.Models.DTO;
using APIServer.Models.DTO.Mail;

namespace APIServer.Service.Implements;

public interface IMailService
{
    Task<Result> SendRewardMail(long userId, string title, long code, int price);
    Task<Result<List<MailData>>> GetMailAsync(long userId, Pageable pageable);
    Task<Result> ReceiveMailAsync(long sessionUserId, long requestMailId);
}