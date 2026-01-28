using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class RedmineJournal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("user")]
    public RedmineUser? User { get; set; }
    
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
    
    [JsonPropertyName("private_notes")]
    public bool PrivateNotes { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonPropertyName("details")]
    public List<JournalDetail> Details { get; set; } = new();
}

public class JournalDetail
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("old_value")]
    public string OldValue { get; set; } = string.Empty;
    
    [JsonPropertyName("new_value")]
    public string NewValue { get; set; } = string.Empty;
}
