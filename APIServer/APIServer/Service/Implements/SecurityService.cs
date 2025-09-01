namespace APIServer.Service.Implements;

using System.Security.Cryptography;
using System.Text;

public class SecurityService : ISecurityService
{
    private const int SaltSize = 16;       // 128-bit
    private const int KeySize  = 32;       // 256-bit
    private const int Iterations = 100_000;

    /// <summary>
    /// PBKDF2(SHA-256)으로 비밀번호를 해싱하는 메서드
    /// 반환값: (성공 여부, 해싱된 비밀번호)
    /// </summary>
    public (bool, string) HashPassword(string? password, string? salt)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(salt))
        {
            return (false, string.Empty);
        }

        var saltBytes = Convert.FromBase64String(salt);
        var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        
        return (true , Convert.ToBase64String(key));
    }

    /// <summary>
    /// 솔트 문자열 생성 메서드 (Base64)
    /// 반환 값 : 솔트 문자열
    /// </summary>
    public string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(salt);
    }

    /// <summary>
    /// 인증 토큰 생성 메서드
    /// 반환 값 : 인증 토큰 문자열
    /// </summary>
    public string GenerateAuthToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32); 
        return ToBase64Url(bytes);
    }

    /// <summary>
    /// 저장된 해시/솔트와 입력 비밀번호로 검증
    /// </summary>
    public bool VerifyPassword(string storedHash, string storedSalt, string inputPassword)
    {
        if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt) || inputPassword is null)
            return false;

        byte[] expectedHash;
        byte[] saltBytes;

        try
        {
            expectedHash = Convert.FromBase64String(storedHash);
            saltBytes = Convert.FromBase64String(storedSalt);
        }
        catch
        {
            return false; // 저장 형식이 깨졌을 때
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var actualHash = pbkdf2.GetBytes(KeySize);

        var result = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        Array.Clear(actualHash, 0, actualHash.Length);
        return result;
    }
    
    private static string ToBase64Url(byte[] data)
    {
        var s = Convert.ToBase64String(data);
        return s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
