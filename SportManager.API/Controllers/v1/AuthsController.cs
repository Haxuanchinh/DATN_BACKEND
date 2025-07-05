using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportManager.Application.Auths;
using SportManager.Application.Auths.Models;
using SportManager.Application.Common.Models;
using SportManager.Application.Users.Models;

namespace SportManager.API.Controllers.v1;

[Route("api/auths")]

[ApiController]
public class AuthsController : ApiControllerBase
{
    [HttpGet("roles")]
    public async Task<PageResult<RoleView>> GetRoles(
        [FromQuery] GetAllRoles request,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(request, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return response;
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<LoginResponse> Login(
        [FromBody] LoginRequest command,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(command, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return response;
    }

    [HttpPost("/logout")]
    public async Task<LogoutResponse> Logout(
        [FromBody] LogoutRequest command,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(command, cancellationToken);
        // await DbContextTransaction.CommitAsync(cancellationToken);
        return response;
    }

    [AllowAnonymous]
    [HttpPost("/refresh-token")]
    public async Task<LoginResponse> RefreshToken(
        [FromBody] RefreshTokenRequest command,
        CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(command, cancellationToken);
        //   await DbContextTransaction.CommitAsync(cancellationToken);
        return response;
    }
}
