using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Categories.Commands.Create;
using SportManager.Application.Categories.Queries;
using SportManager.Application.Category.Commands.Delete;
using SportManager.Application.Category.Commands.Update;
using SportManager.Application.Common.Models;

namespace SportManager.API.Controllers.v1;

[Route("api/categories")]

[ApiController]
public class CategoriesController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<GetQueryByIdQueryResponse> GetDetail(
       [FromQuery] Guid id,
       CancellationToken cancellationToken)
       => await Mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);

    [AllowAnonymous]
    [HttpGet("paging")]
    public async Task<PageResult<GetsPagingCategoryQueryResponse>> Get(
       [FromQuery] GetsPagingQuery request,
       CancellationToken cancellationToken)
       => await Mediator.Send(request, cancellationToken);

    [HttpPost]
    public async Task<CreateCategoryResponse> Create(CreateCategoryCommand command,
        CancellationToken cancellationToken)
        => await Mediator.Send(command, cancellationToken);
    [HttpPut]
    public async Task<UpdateCategoryResponse> Update(UpdateCategoryCommand command,
        CancellationToken cancellationToken)
        => await Mediator.Send(command, cancellationToken);
    [HttpDelete]
    public async Task<int> Delete(DeleteCategoryCommand command,
        CancellationToken cancellationToken)
        => await Mediator.Send(command, cancellationToken);

}
