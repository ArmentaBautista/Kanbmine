using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class RedmineUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
    
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; } = string.Empty;
    
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; } = string.Empty;
    
    [JsonPropertyName("mail")]
    public string Mail { get; set; } = string.Empty;
    
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonPropertyName("last_login_on")]
    public DateTime? LastLoginOn { get; set; }
    
    [JsonIgnore]
    public string FullName => $"{Firstname} {Lastname}".Trim();
    
    [JsonIgnore]
    public bool IsActive => Status == 1;
}

public class UserResponse
{
    [JsonPropertyName("user")]
    public RedmineUser User { get; set; } = new();
}
