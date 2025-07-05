using SportManager.Application.Abstractions;
using SportManager.Application.Auths.Models;
using SportManager.Application.Common.Exception;

namespace SportManager.Application.Auths;

internal class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
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

public class AuthLogoutHandler : IRequestHandler<LogoutRequest, LogoutResponse>
{
    private readonly ITokenService _tokenService;

    public AuthLogoutHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<LogoutResponse> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isRevoked = await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, reason: "User logout");

        return new LogoutResponse
        {
            IsLogoutSucess = isRevoked
        };
    }
}
