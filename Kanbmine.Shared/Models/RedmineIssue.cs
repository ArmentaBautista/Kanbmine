using System.Text.Json.Serialization;

namespace Kanbmine.Shared.Models;

public class RedmineIssue
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("project")]
    public RedmineProject? Project { get; set; }
    
    [JsonPropertyName("tracker")]
    public RedmineTracker? Tracker { get; set; }
    
    [JsonPropertyName("status")]
    public RedmineStatus? Status { get; set; }
    
    [JsonPropertyName("priority")]
    public RedminePriority? Priority { get; set; }
    
    [JsonPropertyName("author")]
    public RedmineUser? Author { get; set; }
    
    [JsonPropertyName("assigned_to")]
    public RedmineUser? AssignedTo { get; set; }
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; set; }
    
    [JsonPropertyName("due_date")]
    public DateOnly? DueDate { get; set; }
    
    [JsonPropertyName("done_ratio")]
    public int DoneRatio { get; set; }
    
    [JsonPropertyName("estimated_hours")]
    public decimal? EstimatedHours { get; set; }
    
    [JsonPropertyName("spent_hours")]
    public decimal? SpentHours { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonPropertyName("updated_on")]
    public DateTime UpdatedOn { get; set; }
    
    [JsonPropertyName("closed_on")]
    public DateTime? ClosedOn { get; set; }
    
    [JsonPropertyName("custom_fields")]
    public List<CustomField> CustomFields { get; set; } = new();
    
    [JsonPropertyName("journals")]
    public List<RedmineJournal> Journals { get; set; } = new();
    
    [JsonPropertyName("attachments")]
    public List<RedmineAttachment> Attachments { get; set; } = new();
}

public class RedmineTracker
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class RedminePriority
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class CustomField
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class IssuesResponse
{
    [JsonPropertyName("issues")]
    public List<RedmineIssue> Issues { get; set; } = new();
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}

public class IssueResponse
{
    [JsonPropertyName("issue")]
    public RedmineIssue Issue { get; set; } = new();
}
