using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Kanbmine.Infrastructure.Exceptions;
using Kanbmine.Shared.Configuration;
using Kanbmine.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kanbmine.Infrastructure.Redmine;

public class RedmineApiClient : IRedmineApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RedmineApiClient> _logger;
    private readonly RedmineConfig _config;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedmineApiClient(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<RedmineApiClient> logger,
        IOptions<RedmineConfig> config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _config = config.Value;
        
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.Timeout);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    #region Autenticación
    
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        try
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{username}:{password}"));
            
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "/users/current.json");
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return AuthResult.Failed("Credenciales incorrectas");
            }
            
            response.EnsureSuccessStatusCode();
            
            var userResponse = await response.Content
                .ReadFromJsonAsync<UserResponse>(_jsonOptions);
            
            if (userResponse?.User == null)
            {
                return AuthResult.Failed("Respuesta inválida del servidor");
            }
            
            _logger.LogInformation(
                "Usuario {Login} autenticado correctamente", 
                userResponse.User.Login);
            
            return AuthResult.Success(
                userResponse.User.ApiKey, 
                userResponse.User);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión con Redmine");
            return AuthResult.Failed("Error de conexión con el servidor");
        }
    }
    
    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "/users/current.json");
            request.Headers.Add("X-Redmine-API-Key", apiKey);
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<RedmineUser> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/users/current.json");
            response.EnsureSuccessStatusCode();
            
            var userResponse = await response.Content
                .ReadFromJsonAsync<UserResponse>(_jsonOptions);
            
            if (userResponse?.User == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            return userResponse.User;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo usuario actual");
            throw new RedmineApiException("Error obteniendo usuario actual", ex);
        }
    }
    
    #endregion
    
    #region Proyectos
    
    public async Task<PagedResult<RedmineProject>> GetProjectsAsync(int offset = 0, int limit = 100)
    {
        var cacheKey = $"projects_{offset}_{limit}";
        
        if (_cache.TryGetValue<PagedResult<RedmineProject>>(cacheKey, out var cached))
        {
            _logger.LogDebug("Proyectos obtenidos del cache");
            return cached!;
        }
        
        try
        {
            var url = $"/projects.json?offset={offset}&limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var projectsResponse = await response.Content
                .ReadFromJsonAsync<ProjectsResponse>(_jsonOptions);
            
            if (projectsResponse == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            var result = new PagedResult<RedmineProject>
            {
                Items = projectsResponse.Projects,
                TotalCount = projectsResponse.TotalCount,
                Limit = projectsResponse.Limit,
                Offset = projectsResponse.Offset
            };
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = 
                    TimeSpan.FromMinutes(_config.CacheDurationMinutes)
            };
            _cache.Set(cacheKey, result, cacheOptions);
            
            _logger.LogInformation("Obtenidos {Count} proyectos", result.Items.Count);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo proyectos");
            throw new RedmineApiException("Error obteniendo proyectos", ex);
        }
    }
    
    public async Task<RedmineProject> GetProjectAsync(string projectId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/projects/{projectId}.json");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var projectElement = doc.RootElement.GetProperty("project");
            var project = JsonSerializer.Deserialize<RedmineProject>(
                projectElement.GetRawText(), _jsonOptions);
            
            if (project == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            return project;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo proyecto {ProjectId}", projectId);
            throw new RedmineApiException($"Error obteniendo proyecto {projectId}", ex);
        }
    }
    
    #endregion
    
    #region Estados
    
    public async Task<List<RedmineStatus>> GetIssueStatusesAsync()
    {
        const string cacheKey = "issue_statuses";
        
        if (_cache.TryGetValue<List<RedmineStatus>>(cacheKey, out var cached))
        {
            _logger.LogDebug("Estados obtenidos del cache");
            return cached!;
        }
        
        try
        {
            var response = await _httpClient.GetAsync("/issue_statuses.json");
            response.EnsureSuccessStatusCode();
            
            var statusesResponse = await response.Content
                .ReadFromJsonAsync<StatusesResponse>(_jsonOptions);
            
            if (statusesResponse == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            // Cachear indefinidamente (los estados raramente cambian)
            _cache.Set(cacheKey, statusesResponse.IssueStatuses);
            
            _logger.LogInformation("Obtenidos {Count} estados", statusesResponse.IssueStatuses.Count);
            
            return statusesResponse.IssueStatuses;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo estados");
            throw new RedmineApiException("Error obteniendo estados", ex);
        }
    }
    
    #endregion
    
    #region Issues
    
    public async Task<PagedResult<RedmineIssue>> GetIssuesAsync(IssueFilter filter)
    {
        var cacheKey = $"issues_{filter.GetCacheKey()}";
        
        if (_cache.TryGetValue<PagedResult<RedmineIssue>>(cacheKey, out var cached))
        {
            _logger.LogDebug("Issues obtenidos del cache: {Key}", cacheKey);
            return cached!;
        }
        
        var queryParams = new List<string>
        {
            $"offset={filter.Offset}",
            $"limit={filter.Limit}"
        };
        
        if (filter.ProjectId.HasValue)
            queryParams.Add($"project_id={filter.ProjectId}");
        
        if (!string.IsNullOrEmpty(filter.StatusId))
            queryParams.Add($"status_id={filter.StatusId}");
        
        if (!string.IsNullOrEmpty(filter.AssignedToId))
            queryParams.Add($"assigned_to_id={filter.AssignedToId}");
        
        if (filter.TrackerId.HasValue)
            queryParams.Add($"tracker_id={filter.TrackerId}");
        
        if (filter.PriorityId.HasValue)
            queryParams.Add($"priority_id={filter.PriorityId}");
        
        if (!string.IsNullOrEmpty(filter.Sort))
            queryParams.Add($"sort={filter.Sort}");
        
        var query = string.Join("&", queryParams);
        var url = $"/issues.json?{query}";
        
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var issuesResponse = await response.Content
                .ReadFromJsonAsync<IssuesResponse>(_jsonOptions);
            
            if (issuesResponse == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            var result = new PagedResult<RedmineIssue>
            {
                Items = issuesResponse.Issues,
                TotalCount = issuesResponse.TotalCount,
                Limit = issuesResponse.Limit,
                Offset = issuesResponse.Offset
            };
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = 
                    TimeSpan.FromMinutes(_config.CacheDurationMinutes)
            };
            _cache.Set(cacheKey, result, cacheOptions);
            
            _logger.LogInformation(
                "Obtenidos {Count} issues de {Total}", 
                result.Items.Count, 
                result.TotalCount);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo issues");
            throw new RedmineApiException("Error obteniendo issues", ex);
        }
    }
    
    public async Task<RedmineIssue> GetIssueAsync(int issueId, IssueInclude[]? include = null)
    {
        var url = $"/issues/{issueId}.json";
        
        if (include != null && include.Length > 0)
        {
            var includeParams = string.Join(",", include.Select(i => i.ToString().ToLower()));
            url += $"?include={includeParams}";
        }
        
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var issueResponse = await response.Content
                .ReadFromJsonAsync<IssueResponse>(_jsonOptions);
            
            if (issueResponse?.Issue == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            return issueResponse.Issue;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error obteniendo issue {IssueId}", issueId);
            throw new RedmineApiException($"Error obteniendo issue {issueId}", ex);
        }
    }
    
    public async Task<RedmineIssue> CreateIssueAsync(CreateIssueRequest request)
    {
        var payload = new
        {
            issue = new
            {
                project_id = request.ProjectId,
                tracker_id = request.TrackerId,
                status_id = request.StatusId,
                priority_id = request.PriorityId,
                subject = request.Subject,
                description = request.Description,
                assigned_to_id = request.AssignedToId,
                start_date = request.StartDate?.ToString("yyyy-MM-dd"),
                due_date = request.DueDate?.ToString("yyyy-MM-dd"),
                estimated_hours = request.EstimatedHours,
                custom_fields = request.CustomFields
            }
        };
        
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync("/issues.json", content);
            
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var errorResponse = await response.Content
                    .ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
                throw new RedmineValidationException(errorResponse?.Errors ?? new List<string> { "Error de validación" });
            }
            
            response.EnsureSuccessStatusCode();
            
            var issueResponse = await response.Content
                .ReadFromJsonAsync<IssueResponse>(_jsonOptions);
            
            if (issueResponse?.Issue == null)
            {
                throw new RedmineApiException("Respuesta inválida del servidor");
            }
            
            InvalidateIssuesCache();
            
            _logger.LogInformation("Issue {IssueId} creado correctamente", issueResponse.Issue.Id);
            
            return issueResponse.Issue;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creando issue");
            throw new RedmineApiException("Error creando issue", ex);
        }
    }
    
    public async Task UpdateIssueAsync(int issueId, UpdateIssueRequest request)
    {
        var payload = new
        {
            issue = new
            {
                status_id = request.StatusId,
                assigned_to_id = request.AssignedToId,
                done_ratio = request.DoneRatio,
                notes = request.Notes,
                private_notes = request.PrivateNotes,
                custom_fields = request.CustomFields
            }
        };
        
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PutAsync(
                $"/issues/{issueId}.json", 
                content);
            
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var errorResponse = await response.Content
                    .ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
                throw new RedmineValidationException(errorResponse?.Errors ?? new List<string> { "Error de validación" });
            }
            
            response.EnsureSuccessStatusCode();
            
            InvalidateIssuesCache();
            
            _logger.LogInformation("Issue {IssueId} actualizado correctamente", issueId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error actualizando issue {IssueId}", issueId);
            throw new RedmineApiException($"Error actualizando issue {issueId}", ex);
        }
    }
    
    public async Task DeleteIssueAsync(int issueId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/issues/{issueId}.json");
            response.EnsureSuccessStatusCode();
            
            InvalidateIssuesCache();
            
            _logger.LogInformation("Issue {IssueId} eliminado correctamente", issueId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error eliminando issue {IssueId}", issueId);
            throw new RedmineApiException($"Error eliminando issue {issueId}", ex);
        }
    }
    
    #endregion
    
    #region Comentarios
    
    public async Task AddCommentAsync(int issueId, string comment, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("El comentario no puede estar vacío", nameof(comment));
        }
        
        var request = new UpdateIssueRequest
        {
            Notes = comment,
            PrivateNotes = isPrivate
        };
        
        await UpdateIssueAsync(issueId, request);
        
        _logger.LogInformation("Comentario agregado al issue {IssueId}", issueId);
    }
    
    #endregion
    
    #region Helper Methods
    
    private void InvalidateIssuesCache()
    {
        // TODO: Implementar invalidación selectiva de cache
        // Por ahora, el cache expirará automáticamente según CacheDurationMinutes
        _logger.LogDebug("Cache de issues invalidado");
    }
    
    #endregion
}
