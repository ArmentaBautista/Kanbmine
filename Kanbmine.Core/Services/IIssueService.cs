using Kanbmine.Shared.Models;

namespace Kanbmine.Core.Services;

public interface IIssueService
{
    Task<PagedResult<RedmineIssue>> GetIssuesAsync(IssueFilter filter);
    Task<RedmineIssue> GetIssueDetailAsync(int issueId);
    Task UpdateIssueStatusAsync(int issueId, int newStatusId, string? comment = null);
    Task AddCommentAsync(int issueId, string comment, bool isPrivate = false);
    Task<List<RedmineStatus>> GetAvailableStatusesAsync();
    Task<PagedResult<RedmineProject>> GetProjectsAsync();
    void InvalidateCache();
}
