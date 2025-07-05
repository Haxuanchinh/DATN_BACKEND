using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SportManager.Application.Abstractions;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(string userId, string username, string? customerId, IEnumerable<string> roles);
    Task<string> GenerateRefreshTokenAsync();
    Task<(ClaimsPrincipal Principal, JwtSecurityToken JwtToken)> ValidateTokenWithoutLifetime(string token);
    ClaimsPrincipal ValidateToken(string token);
}
