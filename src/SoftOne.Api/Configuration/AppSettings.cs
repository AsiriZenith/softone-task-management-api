namespace SoftOne.Api.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ConnectionString { get; set; } = string.Empty;

    public JwtSettings Jwt { get; set; } = new();
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; }
}
