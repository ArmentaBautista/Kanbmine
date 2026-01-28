using Blazored.LocalStorage;
using Kanbmine.Infrastructure.Redmine;
using Kanbmine.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Kanbmine.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRedmineApiClient _redmineClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthenticationService> _logger;
    
    private const string ApiKeyStorageKey = "redmine_api_key";
    private const string UserStorageKey = "redmine_user";
    
    public AuthenticationService(
        IRedmineApiClient redmineClient,
        ILocalStorageService localStorage,
        ILogger<AuthenticationService> logger)
    {
        _redmineClient = redmineClient;
        _localStorage = localStorage;
        _logger = logger;
    }
    
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            var result = await _redmineClient.AuthenticateAsync(username, password);
            
            if (result.IsSuccess && result.User != null)
            {
                // Guardar API Key y usuario en LocalStorage
                await _localStorage.SetItemAsStringAsync(ApiKeyStorageKey, result.ApiKey);
                await _localStorage.SetItemAsync(UserStorageKey, result.User);
                
                _logger.LogInformation("Usuario {Username} autenticado y sesión guardada", username);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login");
            return AuthResult.Failed("Error inesperado durante la autenticación");
        }
    }
    
    public async Task LogoutAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(ApiKeyStorageKey);
            await _localStorage.RemoveItemAsync(UserStorageKey);
            
            _logger.LogInformation("Sesión cerrada correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el logout");
        }
    }
    
    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var apiKey = await _localStorage.GetItemAsStringAsync(ApiKeyStorageKey);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }
            
            // Validar que el API Key siga siendo válido
            var isValid = await _redmineClient.ValidateApiKeyAsync(apiKey);
            
            if (!isValid)
            {
                // API Key inválido, limpiar storage
                await LogoutAsync();
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autenticación");
            return false;
        }
    }
    
    public async Task<RedmineUser?> GetCurrentUserAsync()
    {
        try
        {
            var user = await _localStorage.GetItemAsync<RedmineUser>(UserStorageKey);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo usuario actual del storage");
            return null;
        }
    }
    
    public async Task<string?> GetApiKeyAsync()
    {
        try
        {
            return await _localStorage.GetItemAsStringAsync(ApiKeyStorageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo API Key del storage");
            return null;
        }
    }
}
