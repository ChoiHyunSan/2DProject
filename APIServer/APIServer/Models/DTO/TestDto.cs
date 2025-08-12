using System.ComponentModel.DataAnnotations;

namespace APIServer.Models.DTO;

public class TestRequest : RequestBase
{
    [Required]
    public string name { get; set; }
    
    [Required]
    public string password { get; set; }
}

public class TestResponse
{
    public string message { get; set; }
}