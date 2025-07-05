using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Common.Models;
using SportManager.Application.Brand.Commands.Create;
using SportManager.Application.Brands.Queries;
using SportManager.Application.Brands.Cammands;
using MediatR;

namespace SportManager.API.Controllers.v1;

[Route("api/brands")]

[ApiController]
public class BrandsController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CreateBrandResponse> Create(CreateBrandCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }
    [AllowAnonymous]
    [HttpPut]
    public async Task<Unit> Update(UpdateBrandCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpDelete("{id:guid}")]
    public async Task<Unit> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteBrandCommand(id)
        , cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<GetBrandByIdResponse> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        => await Mediator.Send(new GetBrandByIdQuery(id), cancellationToken);

    [HttpGet("paging")]
    public async Task<PageResult<GetsPagingBrandQueryResponse>> Get(
        [FromQuery] GetsPagingQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);
}
