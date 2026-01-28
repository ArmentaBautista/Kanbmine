using Kanbmine.Shared.Models;

namespace Kanbmine.Core.Services;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<RedmineUser?> GetCurrentUserAsync();
    Task<string?> GetApiKeyAsync();
}
