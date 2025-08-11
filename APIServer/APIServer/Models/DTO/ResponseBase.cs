namespace APIServer.Models.DTO;

public class ResponseBase
{
    public ErrorCode code { get; set; } = ErrorCode.None;
}