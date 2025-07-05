using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportManager.Application.Abstractions;
using SportManager.Domain.Entity;
using System.Security.Claims;

namespace SportManager.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public TokenService(AppDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            throw new ArgumentException("Invalid UserId format.");
        }
        var refreshToken = new RefreshToken
        {
            Token = await _jwtService.GenerateRefreshTokenAsync(),
            UserId = userGuid,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string reason)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken is null || !refreshToken.IsActive)
            return false;

        refreshToken.IsRevoked = true;
        //refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedReason = reason;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var storedToken = await GetRefreshTokenAsync(refreshToken);
        if (storedToken == null || !storedToken.IsActive)
            throw new ApplicationException("Invalid or expired refresh token");

        var (principal, jwtToken) = await _jwtService.ValidateTokenWithoutLifetime(accessToken);
        if (principal == null || jwtToken == null)
            throw new ApplicationException("Invalid access token");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var customerId = principal.FindFirstValue("customer_id");
        var username = principal.FindFirstValue(ClaimTypes.Name);
        var roles = principal.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        if (storedToken.UserId.ToString() != userId)
            throw new ApplicationException("Token does not match the user");

        // Revoke old refresh token
        storedToken.IsRevoked = true;
        // storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedReason = "Replaced by new token";

        var newAccessToken = await _jwtService.GenerateTokenAsync(userId!, username!, customerId, roles);
        var newRefreshToken = await CreateRefreshTokenAsync(userId!);

        storedToken.ReplacedByToken = newRefreshToken.Token;

        await _dbContext.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }
}
