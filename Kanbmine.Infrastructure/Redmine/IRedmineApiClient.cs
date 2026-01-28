using Kanbmine.Shared.Models;

namespace Kanbmine.Infrastructure.Redmine;

public interface IRedmineApiClient
{
    // Autenticación
    Task<AuthResult> AuthenticateAsync(string username, string password);
    Task<bool> ValidateApiKeyAsync(string apiKey);
    Task<RedmineUser> GetCurrentUserAsync();
    
    // Proyectos
    Task<PagedResult<RedmineProject>> GetProjectsAsync(int offset = 0, int limit = 100);
    Task<RedmineProject> GetProjectAsync(string projectId);
    
    // Estados
    Task<List<RedmineStatus>> GetIssueStatusesAsync();
    
    // Issues
    Task<PagedResult<RedmineIssue>> GetIssuesAsync(IssueFilter filter);
    Task<RedmineIssue> GetIssueAsync(int issueId, IssueInclude[]? include = null);
    Task<RedmineIssue> CreateIssueAsync(CreateIssueRequest request);
    Task UpdateIssueAsync(int issueId, UpdateIssueRequest request);
    Task DeleteIssueAsync(int issueId);
    
    // Comentarios (via journals)
    Task AddCommentAsync(int issueId, string comment, bool isPrivate = false);
}
