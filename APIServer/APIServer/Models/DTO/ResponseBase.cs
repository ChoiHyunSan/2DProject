using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class RequestBase
{
    [Required(ErrorMessage = "email is required")]
    [EmailAddress(ErrorMessage = "email is invalid")]
    public string email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Auth Token is required")]
    public string authToken { get; set; } = string.Empty;
}

public class ResponseBase
{
    public ErrorCode code { get; set; } = ErrorCode.None;
}