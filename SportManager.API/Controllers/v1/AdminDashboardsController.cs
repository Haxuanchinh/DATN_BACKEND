using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportManager.Application.AdminDashboards.Models;
using SportManager.Application.AdminDashboards.Queries;

namespace SportManager.API.Controllers.v1;

[Route("api/admin-dashboards")]

[ApiController]
public class AdminDashboardsController : ApiControllerBase
{
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("chart")]
    public async Task<ModelDashboardAdminResponse> Get(
        [FromQuery] GetsChartAndRpQuery request,
        CancellationToken cancellationToken)
        => await Mediator.Send(request, cancellationToken);
}
