using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Carts.Commands.Delete;
using SportManager.Application.Carts.Commands.Update;
using SportManager.Application.Carts.Queries;
using SportManager.Application.Carts.Models;
using SportManager.Application.Carts.Commands.Create;

namespace SportManager.API.Controllers.v1;

[Route("api/carts")]
[ApiController]
public class CartsController : ApiControllerBase
{
    //[Authorize(Policy = "ADMIN")]
    [HttpPost]
    public async Task<AddToCartResponse> Create(AddToCartCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpPut("update-quantity")]
    public async Task<bool> UpdateQty(UpdateCartQuantityCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    //[Authorize(Policy = "ADMIN")]
    [HttpPut]
    public async Task<bool> Update(UpdateCartCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }
    //[Authorize(Policy = "ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<int> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteCartCommand(id), cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpGet]
    public async Task<List<CartItemDto>> GetById(CancellationToken cancellationToken)
        => await Mediator.Send(new GetCartQuery(), cancellationToken);

}
