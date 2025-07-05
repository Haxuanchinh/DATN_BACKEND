using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Common.Models;
using SportManager.Application.Supplier.Commands.Create;
using SportManager.Application.Suppliers.Queries;

namespace SportManager.API.Controllers.v1;

[Route("api/suppliers")]

[ApiController]
public class SuppliersController : ApiControllerBase
{
    [HttpPost]
    public async Task<CreateSupplierResponse> Create(CreateSupplierCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }
    [HttpPut]
    public async Task<Unit> Update(UpdateSupplierCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpDelete("{id:guid}")]
    public async Task<Unit> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteSupplierCommand(id), cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<GetSupplierByIdResponse> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        => await Mediator.Send(new GetSupplierByIdQuery(id), cancellationToken);

    [HttpGet("paging")]
    public async Task<PageResult<GetsPagingSupplierQueryResponse>> Get(
        [FromQuery] GetsPagingQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);
}
