using SportManager.Domain.Entity;

namespace SportManager.Application.Abstractions;

public interface ITokenService
{
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task<RefreshToken> CreateRefreshTokenAsync(string userId);
    Task<bool> RevokeRefreshTokenAsync(string token, string reason);
    Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken);
}
