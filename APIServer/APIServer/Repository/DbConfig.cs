namespace APIServer.Config;

/// <summary>
/// Db 연결 정보 관련 Config
/// </summary>
public class DbConfig
{
    public string  AccountDb { get; set; } = string.Empty;
    public string GameDb { get; set; } = string.Empty;
    public string MasterDb { get; set; } = string.Empty;

    public string Redis { get; set; } = string.Empty;
}