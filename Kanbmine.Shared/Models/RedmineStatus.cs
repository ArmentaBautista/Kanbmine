using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class RedmineStatus
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("is_closed")]
    public bool IsClosed { get; set; }
}

public class StatusesResponse
{
    [JsonPropertyName("issue_statuses")]
    public List<RedmineStatus> IssueStatuses { get; set; } = new();
}
