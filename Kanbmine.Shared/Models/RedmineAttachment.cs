using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class RedmineAttachment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;
    
    [JsonPropertyName("filesize")]
    public long Filesize { get; set; }
    
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public RedmineUser? Author { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonPropertyName("content_url")]
    public string ContentUrl { get; set; } = string.Empty;
}
