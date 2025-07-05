using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using SportManager.Application.Abstractions;

namespace SportManager.API.Controllers;


[Authorize]
public class ApiControllerBase : ApiControllerBase_
{
    public ApiControllerBase(ILogger? logger = null) : base(logger)
    {
    }
}

[AllowAnonymous]
public class ApiControllerBaseInternal : ApiControllerBase_
{
    public ApiControllerBaseInternal(ILogger? logger = null) : base(logger)
    {
    }
}

public abstract class ApiControllerBase_ : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    private IDbContextTransaction _dbContextTransaction = null!;
    private ICurrentUserService? _currentUser;
    private readonly ILogger? _logger;

    protected ICurrentUserService CurrentUser =>
       _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    protected ApiControllerBase_(ILogger? logger = null)
    {
        _logger = logger;
    }

    protected IDbContextTransaction DbContextTransaction
    {
        get
        {
            if (_dbContextTransaction is null)
                _dbContextTransaction = HttpContext.RequestServices.GetRequiredService<IDbContextTransaction>();

            return _dbContextTransaction;
        }
    }

    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> command,
        CancellationToken cancellationToken,
        Func<Task>? onPostProcess = null)
    {
        var response = await Mediator.Send(command, cancellationToken);
        await DbContextTransaction.CommitAsync(cancellationToken);
        if (onPostProcess != null)
        {
            await onPostProcess.Invoke();
        }

        return response;
    }
}
