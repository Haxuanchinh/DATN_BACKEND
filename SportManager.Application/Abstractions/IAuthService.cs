using SportManager.Application.Auths.Models;

namespace SportManager.Application.Abstractions;

public interface IAuthService
{
    Task<(bool Success, string? UserId, string? Username, string? CustomerId, int totalCartItems, IEnumerable<string>? Roles)> ValidateUserAsync(LoginRequest request);
    public string HashPassword(string password, string username);
}
