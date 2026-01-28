namespace Kanbmine.Shared.Configuration;

public class RedmineConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    public int CacheDurationMinutes { get; set; } = 5;
    public int MaxRetries { get; set; } = 3;
    public int PageSize { get; set; } = 100;
}
