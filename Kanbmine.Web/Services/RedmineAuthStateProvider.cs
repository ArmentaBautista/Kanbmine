using System.Security.Claims;
using Kanbmine.Core.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Kanbmine.Web.Services;

public class RedmineAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthenticationService _authService;
    
    public RedmineAuthStateProvider(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        
        if (!isAuthenticated)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        var user = await _authService.GetCurrentUserAsync();
        
        if (user == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Mail),
            new Claim("FullName", user.FullName)
        };
        
        var identity = new ClaimsIdentity(claims, "RedmineAuth");
        var principal = new ClaimsPrincipal(identity);
        
        return new AuthenticationState(principal);
    }
    
    public void NotifyUserAuthentication(string username)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };
        
        var identity = new ClaimsIdentity(claims, "RedmineAuth");
        var principal = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
    }
    
    public void NotifyUserLogout()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
    }
}
