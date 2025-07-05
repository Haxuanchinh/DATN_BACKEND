using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SportManager.Application.Abstractions;
using SportManager.Application.Auths.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using System.Security.Claims;

namespace SportManager.Application.Auths;

internal class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotNull()
            .NotEmpty()
            .WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.RefreshToken)
            .NotNull()
            .NotEmpty()
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class AuthRefreshTokenHandler : IRequestHandler<RefreshTokenRequest, LoginResponse>
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IReadOnlyApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthRefreshTokenHandler(
        IJwtService jwtService,
        IReadOnlyApplicationDbContext dbContext,
        IAuthService authService,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _authService = authService;
        _dbContext = dbContext;
        _jwtService = jwtService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(
                request.AccessToken, request.RefreshToken);

            // Validate new access token
            var principal = _jwtService.ValidateToken(newAccessToken);
            if (principal == null)
                throw new SecurityTokenException("Invalid token.");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userId == null || username == null)
                throw new SecurityTokenException("Invalid token payload.");

            // Lấy thời gian hết hạn từ config
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"]));

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                UserId = userId,
                Username = username,
                Roles = roles,
                ExpiresAt = expiresAt
            };
        }
        catch (SecurityTokenException ex)
        {
            throw new ApplicationException($"Token validation failed: {ex.Message}");
        }
    }
}
