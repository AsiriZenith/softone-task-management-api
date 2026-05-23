namespace SoftOne.Api.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ApplicationName { get; set; } = "SoftOne";

    public string Environment { get; set; } = "Development";
}
