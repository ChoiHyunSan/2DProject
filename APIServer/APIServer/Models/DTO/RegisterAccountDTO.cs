using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public record RegisterAccountRequest
{
    [Required(ErrorMessage = "email is required")]
    [EmailAddress(ErrorMessage = "email is invalid")]
    public string email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "password is required")]
    [MinLength(8, ErrorMessage = "password`s length must be greater than 8")]
    public string password { get; set; } = string.Empty;
}

public class RegisterAccountResponse : ResponseBase
{
    
}