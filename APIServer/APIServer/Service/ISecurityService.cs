namespace APIServer.Service;

public interface ISecurityService
{
    (bool, string) HashPassword(string? password, string? salt);
    string GenerateSalt();
    string GenerateAuthToken();
    bool VerifyPassword(string storedHash, string storedSalt, string inputPassword);
}