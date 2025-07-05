using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Common.Models;
using SportManager.Application.Products.Commands.Create;
using SportManager.Application.Products.Commands.Delete;
using SportManager.Application.Products.Quries;
using SportManager.Application.Products.Commands.Update;
using SportManager.Application.ProductReviews.Commands;
using SportManager.Application.Products.Queries;

namespace SportManager.API.Controllers.v1;

[Route("api/products")]

[ApiController]
public class ProductsController : ApiControllerBase
{
    // [Authorize(Policy = "ADMIN")]
    [HttpPost]
    public async Task<CreateProductResponse> Create(CreateProductCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }


    [HttpPost("review")]
    public async Task<Guid> ProductReview(CreateProductReviewCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpPost("comment")]
    public async Task<Guid> ProductReviewComment(CreateProductReviewCommentCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [HttpGet("{productId}/reviews")]
    public async Task<IActionResult> GetProductReviews(Guid productId)
    {
        var reviews = await Mediator.Send(new GetProductReviewsQuery(productId));
        return Ok(reviews);
    }

    //[Authorize(Policy = "ADMIN")]
    [HttpPut]
    public async Task<UpdateCommandResponse> Update([FromBody] UpdateProductCommand input, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(input, cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }
    //[Authorize(Policy = "ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<int> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id), cancellationToken);
        //  await DbContextTransaction.CommitAsync(cancellationToken);
        return result;
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("inventory-low")]
    public async Task<List<ProductInventoryLowDto>> GetProductInventoryLow(CancellationToken cancellationToken)
        => await Mediator.Send(new GetProductInventoryLowQuery(), cancellationToken);

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ProductPageResponse> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        => await Mediator.Send(new GetProductByIdQuery(id), cancellationToken);

    [AllowAnonymous]
    [HttpGet("paging")]
    public async Task<PageResult<ProductPageResponse>> Get(
        [FromQuery] GetsPagingQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);

    [AllowAnonymous]
    [HttpGet("variant-paging")]
    public async Task<PaginatedProductVariantsResult> GetVariants(
        [FromQuery] GetAllProductVariantsQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);

}
