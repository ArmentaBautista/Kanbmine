# Especificación de Implementación - Cliente API Redmine
## Kanbmine - Blazor Server + .NET 10

---

## 1. Resumen Ejecutivo

Este documento detalla la implementación del cliente HTTP para integrar Kanbmine con la API REST de Redmine, basado en el análisis de [REDMINE_API_ENDPOINTS.md](../REDMINE_API_ENDPOINTS.md).

**Características Clave:**
- Autenticación con HTTP Basic Auth (login) → API Key (sesión)
- Gestión completa de issues (CRUD)
- Drag & drop con actualización optimista
- Manejo de comentarios (journals)
- Cache inteligente con invalidación
- Manejo robusto de errores

---

## 2. Arquitectura del Cliente API

### 2.1 Estructura de Capas

```
Kanbmine.Infrastructure/
├── Redmine/
│   ├── IRedmineApiClient.cs          # Interfaz principal
│   ├── RedmineApiClient.cs           # Implementación HttpClient
│   ├── RedmineAuthHandler.cs         # DelegatingHandler para API Key
│   ├── Models/                       # DTOs de Redmine
│   │   ├── RedmineUser.cs
│   │   ├── RedmineIssue.cs
│   │   ├── RedmineProject.cs
│   │   ├── RedmineStatus.cs
│   │   └── RedmineJournal.cs
│   └── Exceptions/
│       ├── RedmineAuthException.cs
│       └── RedmineApiException.cs
```

### 2.2 Configuración (appsettings.json)

```json
{
  "Redmine": {
    "BaseUrl": "https://redmine.example.com",
    "Timeout": 30,
    "CacheDurationMinutes": 5,
    "MaxRetries": 3,
    "PageSize": 100
  }
}
```

---

## 3. Interfaz IRedmineApiClient

```csharp
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
    Task<RedmineIssue> GetIssueAsync(int issueId, IssueInclude? include = null);
    Task<RedmineIssue> CreateIssueAsync(CreateIssueRequest request);
    Task UpdateIssueAsync(int issueId, UpdateIssueRequest request);
    Task DeleteIssueAsync(int issueId);
    
    // Comentarios (via journals)
    Task AddCommentAsync(int issueId, string comment, bool isPrivate = false);
}
```

---

## 4. Modelos DTO

### 4.1 RedmineUser.cs

```csharp
public class RedmineUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; }
    
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; }
    
    [JsonPropertyName("mail")]
    public string Mail { get; set; }
    
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    
    [JsonPropertyName("last_login_on")]
    public DateTime? LastLoginOn { get; set; }
    
    [JsonIgnore]
    public string FullName => $"{Firstname} {Lastname}";
    
    [JsonIgnore]
    public bool IsActive => Status == 1;
}

public class UserResponse
{
    [JsonPropertyName("user")]
    public RedmineUser User { get; set; }
}
```

### 4.2 RedmineIssue.cs

```csharp
public class RedmineIssue
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("project")]
    public RedmineProject Project { get; set; }
    
    [JsonPropertyName("tracker")]
    public RedmineTracker Tracker { get; set; }
    
    [JsonPropertyName("status")]
    public RedmineStatus Status { get; set; }
    
    [JsonPropertyName("priority")]
    public RedminePriority Priority { get; set; }
    
    [JsonPropertyName("author")]
    public RedmineUser Author { get; set; }
    
    [JsonPropertyName("assigned_to")]
    public RedmineUser AssignedTo { get; set; }
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
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

public class IssuesResponse
{
    [JsonPropertyName("issues")]
    public List<RedmineIssue> Issues { get; set; }
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}
```

### 4.3 RedmineStatus.cs

```csharp
public class RedmineStatus
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("is_closed")]
    public bool IsClosed { get; set; }
}

public class StatusesResponse
{
    [JsonPropertyName("issue_statuses")]
    public List<RedmineStatus> IssueStatuses { get; set; }
}
```

### 4.4 RedmineJournal.cs

```csharp
public class RedmineJournal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("user")]
    public RedmineUser User { get; set; }
    
    [JsonPropertyName("notes")]
    public string Notes { get; set; }
    
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
    public string Property { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("old_value")]
    public string OldValue { get; set; }
    
    [JsonPropertyName("new_value")]
    public string NewValue { get; set; }
}
```

---

## 5. Implementación del Cliente

### 5.1 RedmineApiClient.cs (Esqueleto)

```csharp
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
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    // Métodos de implementación...
}
```

### 5.2 Autenticación

```csharp
public async Task<AuthResult> AuthenticateAsync(string username, string password)
{
    try
    {
        // Crear credenciales Base64
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{username}:{password}"));
        
        // Crear request con Basic Auth
        using var request = new HttpRequestMessage(
            HttpMethod.Get, "/users/current.json");
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return AuthResult.Failed("Credenciales incorrectas");
        }
        
        response.EnsureSuccessStatusCode();
        
        var userResponse = await response.Content
            .ReadFromJsonAsync<UserResponse>(_jsonOptions);
        
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
```

### 5.3 Obtener Issues con Filtros

```csharp
public async Task<PagedResult<RedmineIssue>> GetIssuesAsync(IssueFilter filter)
{
    var cacheKey = $"issues_{filter.GetCacheKey()}";
    
    // Intentar obtener del cache
    if (_cache.TryGetValue<PagedResult<RedmineIssue>>(cacheKey, out var cached))
    {
        _logger.LogDebug("Issues obtenidos del cache: {Key}", cacheKey);
        return cached;
    }
    
    // Construir query string
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
        
        var result = new PagedResult<RedmineIssue>
        {
            Items = issuesResponse.Issues,
            TotalCount = issuesResponse.TotalCount,
            Limit = issuesResponse.Limit,
            Offset = issuesResponse.Offset
        };
        
        // Guardar en cache
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
```

### 5.4 Actualizar Issue (Drag & Drop)

```csharp
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
            throw new RedmineValidationException(errorResponse.Errors);
        }
        
        response.EnsureSuccessStatusCode();
        
        // Invalidar cache de issues
        InvalidateIssuesCache();
        
        _logger.LogInformation(
            "Issue {IssueId} actualizado correctamente", 
            issueId);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Error actualizando issue {IssueId}", issueId);
        throw new RedmineApiException($"Error actualizando issue {issueId}", ex);
    }
}

private void InvalidateIssuesCache()
{
    // Implementar invalidación de cache por patrón
    // En .NET 10, MemoryCache no tiene RemoveByPattern
    // Alternativa: usar un sistema de tags o versionado
}
```

### 5.5 Agregar Comentario

```csharp
public async Task AddCommentAsync(
    int issueId, 
    string comment, 
    bool isPrivate = false)
{
    if (string.IsNullOrWhiteSpace(comment))
        throw new ArgumentException("El comentario no puede estar vacío");
    
    var request = new UpdateIssueRequest
    {
        Notes = comment,
        PrivateNotes = isPrivate
    };
    
    await UpdateIssueAsync(issueId, request);
    
    _logger.LogInformation(
        "Comentario agregado al issue {IssueId}", 
        issueId);
}
```

---

## 6. Configuración de Inyección de Dependencias

### 6.1 Program.cs

```csharp
// Configuración
builder.Services.Configure<RedmineConfig>(
    builder.Configuration.GetSection("Redmine"));

// HttpClient con Polly (retry policy)
builder.Services.AddHttpClient<IRedmineApiClient, RedmineApiClient>()
    .AddHttpMessageHandler<RedmineAuthHandler>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(
            3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

// Memory Cache
builder.Services.AddMemoryCache();

// Auth Handler
builder.Services.AddScoped<RedmineAuthHandler>();
```

### 6.2 RedmineAuthHandler.cs

```csharp
public class RedmineAuthHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    
    public RedmineAuthHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Agregar API Key a todas las peticiones
        var apiKey = await _localStorage.GetItemAsync<string>("redmine_api_key");
        
        if (!string.IsNullOrEmpty(apiKey) && 
            !request.Headers.Contains("X-Redmine-API-Key"))
        {
            request.Headers.Add("X-Redmine-API-Key", apiKey);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## 7. Manejo de Errores

### 7.1 Excepciones Personalizadas

```csharp
public class RedmineApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    
    public RedmineApiException(string message) : base(message) { }
    
    public RedmineApiException(string message, Exception inner) 
        : base(message, inner) { }
    
    public RedmineApiException(string message, HttpStatusCode statusCode) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class RedmineAuthException : RedmineApiException
{
    public RedmineAuthException(string message) : base(message) { }
}

public class RedmineValidationException : RedmineApiException
{
    public List<string> Errors { get; }
    
    public RedmineValidationException(List<string> errors) 
        : base("Error de validación")
    {
        Errors = errors;
    }
}
```

### 7.2 ErrorResponse.cs

```csharp
public class ErrorResponse
{
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}
```

---

## 8. Modelos de Request

### 8.1 IssueFilter.cs

```csharp
public class IssueFilter
{
    public int? ProjectId { get; set; }
    public string StatusId { get; set; } = "open"; // open, closed, *
    public string AssignedToId { get; set; } // me, user_id
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
```

### 8.2 UpdateIssueRequest.cs

```csharp
public class UpdateIssueRequest
{
    public int? StatusId { get; set; }
    public int? AssignedToId { get; set; }
    public int? DoneRatio { get; set; }
    public string Notes { get; set; }
    public bool PrivateNotes { get; set; }
    public List<CustomField> CustomFields { get; set; }
}
```

### 8.3 CreateIssueRequest.cs

```csharp
public class CreateIssueRequest
{
    public int ProjectId { get; set; }
    public int TrackerId { get; set; }
    public int StatusId { get; set; } = 1; // New
    public int PriorityId { get; set; } = 4; // Normal
    public string Subject { get; set; }
    public string Description { get; set; }
    public int? AssignedToId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public List<CustomField> CustomFields { get; set; }
}
```

---

## 9. Resultado de Operaciones

### 9.1 AuthResult.cs

```csharp
public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string ApiKey { get; private set; }
    public RedmineUser User { get; private set; }
    public string ErrorMessage { get; private set; }
    
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
```

### 9.2 PagedResult.cs

```csharp
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
```

---

## 10. Testing

### 10.1 Tests Unitarios

```csharp
public class RedmineApiClientTests
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly RedmineApiClient _client;
    
    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var responseJson = @"{
            ""user"": {
                ""id"": 1,
                ""login"": ""admin"",
                ""api_key"": ""test123""
            }
        }";
        
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });
        
        // Act
        var result = await _client.AuthenticateAsync("admin", "admin");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test123", result.ApiKey);
    }
    
    [Fact]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsFailed()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });
        
        // Act
        var result = await _client.AuthenticateAsync("invalid", "invalid");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Credenciales", result.ErrorMessage);
    }
}
```

---

## 11. Flujo de Implementación Recomendado

1. **Fase 1:** Implementar modelos DTO básicos
2. **Fase 2:** Implementar autenticación (AuthenticateAsync, ValidateApiKeyAsync)
3. **Fase 3:** Implementar GetIssueStatusesAsync (para columnas del board)
4. **Fase 4:** Implementar GetProjectsAsync (para filtros)
5. **Fase 5:** Implementar GetIssuesAsync con filtros y cache
6. **Fase 6:** Implementar GetIssueAsync con journals (detalle completo)
7. **Fase 7:** Implementar UpdateIssueAsync (drag & drop)
8. **Fase 8:** Implementar AddCommentAsync
9. **Fase 9:** Agregar tests unitarios
10. **Fase 10:** Agregar retry policies y circuit breaker

---

## 12. Consideraciones Importantes

### 12.1 Seguridad
- ✅ Nunca exponer API Key en logs
- ✅ Usar HTTPS en producción
- ✅ Almacenar API Key en LocalStorage con encriptación
- ✅ Validar entrada de usuario antes de enviar a API
- ✅ Implementar rate limiting en cliente

### 12.2 Performance
- ✅ Cache de 5 minutos para issues
- ✅ Cache indefinido para issue_statuses (raramente cambian)
- ✅ Paginación con limit=100 (máximo permitido)
- ✅ Actualización optimista en UI para drag & drop
- ✅ Lazy loading de journals/attachments

### 12.3 Manejo de Errores
- ✅ Retry automático con backoff exponencial
- ✅ Circuit breaker para evitar cascadas de fallos
- ✅ Mensajes de error amigables al usuario
- ✅ Logging estructurado para debugging
- ✅ Rollback de UI si falla actualización

---

## 13. Siguiente Paso

Con esta especificación completa, el siguiente paso es:

1. Crear la estructura de carpetas en `Kanbmine.Infrastructure`
2. Implementar los modelos DTO
3. Implementar `RedmineApiClient` siguiendo esta especificación
4. Configurar DI en `Program.cs`
5. Crear tests unitarios básicos

**Referencia:** [PLAN_TRABAJO.md - Fase 2](../PLAN_TRABAJO.md#fase-2-integración-con-api-de-redmine)
