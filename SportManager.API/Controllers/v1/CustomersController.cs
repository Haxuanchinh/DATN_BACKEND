using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Common.Models;
using SportManager.Application.Customer.Commands.Create;
using SportManager.Application.Customer.Commands.Delete;
using SportManager.Application.Customer.Commands.Update;
using SportManager.Application.Customer.Queries;
using SportManager.Application.Users.Models;

namespace SportManager.API.Controllers.v1;

[Route("api/customers")]

[ApiController]
public class CustomersController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CreateCustomerResponse> Create(CreateCustomerCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpPut]
    public async Task<UpdateCustomerResponse> Update(UpdateCustomerCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpPut("assign-roles")]
    public async Task<Unit> UpdateRole(UpdateRolesCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpDelete("{id:guid}")]
    public async Task<int> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteCustomerCommand(id), cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<GetUserResponse> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        => await Mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);

    [HttpGet("current")]
    public async Task<GetUserResponse> GetCurrentCustomer(CancellationToken cancellationToken)
        => await Mediator.Send(new GetCurrentCustomerQuery(), cancellationToken);

    [HttpGet("paging")]
    public async Task<PageResult<GetUserResponse>> Get(
        [FromQuery] GetsPagingQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);
}
