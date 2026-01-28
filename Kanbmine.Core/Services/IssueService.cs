using Kanbmine.Infrastructure.Redmine;
using Kanbmine.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Kanbmine.Core.Services;

public class IssueService : IIssueService
{
    private readonly IRedmineApiClient _redmineClient;
    private readonly ILogger<IssueService> _logger;
    
    public IssueService(
        IRedmineApiClient redmineClient,
        ILogger<IssueService> logger)
    {
        _redmineClient = redmineClient;
        _logger = logger;
    }
    
    public async Task<PagedResult<RedmineIssue>> GetIssuesAsync(IssueFilter filter)
    {
        try
        {
            return await _redmineClient.GetIssuesAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo issues con filtro");
            throw;
        }
    }
    
    public async Task<RedmineIssue> GetIssueDetailAsync(int issueId)
    {
        try
        {
            var include = new[]
            {
                IssueInclude.Journals,
                IssueInclude.Attachments,
                IssueInclude.Watchers
            };
            
            return await _redmineClient.GetIssueAsync(issueId, include);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo detalle del issue {IssueId}", issueId);
            throw;
        }
    }
    
    public async Task UpdateIssueStatusAsync(int issueId, int newStatusId, string? comment = null)
    {
        try
        {
            var request = new UpdateIssueRequest
            {
                StatusId = newStatusId,
                Notes = comment
            };
            
            await _redmineClient.UpdateIssueAsync(issueId, request);
            
            _logger.LogInformation(
                "Estado del issue {IssueId} actualizado a {StatusId}", 
                issueId, 
                newStatusId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando estado del issue {IssueId}", issueId);
            throw;
        }
    }
    
    public async Task AddCommentAsync(int issueId, string comment, bool isPrivate = false)
    {
        try
        {
            await _redmineClient.AddCommentAsync(issueId, comment, isPrivate);
            
            _logger.LogInformation("Comentario agregado al issue {IssueId}", issueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error agregando comentario al issue {IssueId}", issueId);
            throw;
        }
    }
    
    public async Task<List<RedmineStatus>> GetAvailableStatusesAsync()
    {
        try
        {
            var statuses = await _redmineClient.GetIssueStatusesAsync();
            
            // Filtrar solo estados abiertos para el tablero Kanban
            return statuses.Where(s => !s.IsClosed).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estados disponibles");
            throw;
        }
    }
    
    public async Task<PagedResult<RedmineProject>> GetProjectsAsync()
    {
        try
        {
            return await _redmineClient.GetProjectsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo proyectos");
            throw;
        }
    }
    
    public void InvalidateCache()
    {
        // TODO: Implementar invalidación de cache si es necesario
        _logger.LogInformation("Cache invalidado manualmente");
    }
}
