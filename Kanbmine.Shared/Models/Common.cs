namespace Kanbmine.Shared.Models;

public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public RedmineUser? User { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    
    public static AuthResult Success(string apiKey, RedmineUser user) => new()
    {
        IsSuccess = true,
        ApiKey = apiKey,
        User = user
    };
    
    public static AuthResult Failed(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    
    public int CurrentPage => (Offset / Limit) + 1;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Limit);
    public bool HasNextPage => Offset + Limit < TotalCount;
    public bool HasPreviousPage => Offset > 0;
}

public class IssueFilter
{
    public int? ProjectId { get; set; }
    public string StatusId { get; set; } = "open"; // open, closed, *
    public string? AssignedToId { get; set; } // me, user_id
    public int? TrackerId { get; set; }
    public int? PriorityId { get; set; }
    public string Sort { get; set; } = "updated_on:desc";
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 100;
    
    public string GetCacheKey()
    {
        return $"{ProjectId}_{StatusId}_{AssignedToId}_{Sort}_{Offset}_{Limit}";
    }
}

public class UpdateIssueRequest
{
    public int? StatusId { get; set; }
    public int? AssignedToId { get; set; }
    public int? DoneRatio { get; set; }
    public string? Notes { get; set; }
    public bool PrivateNotes { get; set; }
    public List<CustomField>? CustomFields { get; set; }
}

public class CreateIssueRequest
{
    public int ProjectId { get; set; }
    public int TrackerId { get; set; }
    public int StatusId { get; set; } = 1; // New
    public int PriorityId { get; set; } = 4; // Normal
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedToId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public List<CustomField>? CustomFields { get; set; }
}

public enum IssueInclude
{
    Children,
    Attachments,
    Relations,
    Changesets,
    Journals,
    Watchers,
    AllowedStatuses
}
