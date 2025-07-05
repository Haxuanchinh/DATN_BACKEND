using Microsoft.Extensions.Configuration;
using SportManager.Application.Abstractions;
using SportManager.Application.Auths.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Auths;

internal class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotNull()
            .NotEmpty()
            .WithMessage(ErrorCode.FIELD_REQUIRED)
            .MaximumLength(200)
            .WithMessage(ErrorCode.MAX_LENGTH_200);

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class AuthLoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IReadOnlyApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthLoginHandler(
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

    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (success, userId, username, customerId, totalCartItems, roles) = await _authService.ValidateUserAsync(request);

        if (!success)
            throw new ApplicationException("Username or password is incorrect");

        // Tạo access token
        var accessToken = await _jwtService.GenerateTokenAsync(userId, username, customerId, roles);

        // Tạo refresh token
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(userId);

        // Lấy thời gian hết hạn từ cấu hình
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"]));

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            UserId = userId,
            Username = username,
            TotalCartItems = totalCartItems,
            Roles = roles,
            ExpiresAt = expiresAt
        };
    }
}
