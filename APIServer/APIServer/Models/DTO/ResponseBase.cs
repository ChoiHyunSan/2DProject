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

public class PageableBase : RequestBase
{
    [Required]
    public Pageable Pageable { get; set; } = new();
}

public record Pageable
{
    [Required]
    public int page { get; set; } = 1;
    [Required]
    public int size { get; set; } = 10;
    
    public static List<T> Pagination<T>(List<T> list, Pageable pageable)
    {
        var page = pageable.page;
        var pageSize = pageable.size;
        
        var skip = (page - 1) * pageSize;
        return list.Skip(skip).Take(pageSize).ToList();
    }
}


public class ResponseBase
{
    public ErrorCode code { get; set; } = ErrorCode.None;
}