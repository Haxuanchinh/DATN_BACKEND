using Microsoft.AspNetCore.Mvc;
using SportManager.Application.Carts.Commands.Delete;
using SportManager.Application.Carts.Commands.Update;
using SportManager.Application.VoucherManagements.Commands;
using SportManager.Application.VoucherManagements.Models;
using Microsoft.AspNetCore.Authorization;
using SportManager.Application.VoucherManagements.Queries;
using SportManager.Application.Abstractions;
using SportManager.Application.Orders.Queries;

namespace SportManager.API.Controllers.v1;

[Route("api/vouchers")]
[ApiController]
public class VouchersController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;


    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllVouchers([FromQuery] GetAllVouchersQuery request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    [HttpGet("get-available")]
    public async Task<IActionResult> GetAllAvailableVouchers([FromQuery] GetAvailableVouchersQuery request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    public VouchersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Guid>> CreateVoucher([FromBody] CreateVoucherCommand request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<ActionResult<Guid>> UpdateVoucher([FromBody] UpdateVoucherCommand request)
    {
        var result = await Mediator.Send(request);
        return Ok(result);
    }

    // POST /api/vouchers/apply
    [HttpPost("apply")]
    [Authorize]
    public async Task<ActionResult<ApplyVoucherResultDto>> ApplyVoucher(
        [FromBody] ApplyVoucherRequestDto request)
    {
        var userId = Guid.Parse(_currentUserService.UserId);

        var result = await Mediator.Send(new ApplyVoucherCommand
        {
            VoucherCode = request.VoucherCode,
            OrderId = request.OrderId,
            UserId = userId
        });

        return Ok(result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateVoucherCommand command)
    {
        if (id != command.Id) return BadRequest("Mismatched ID");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteVoucherCommand { Id = id });
        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<VoucherDto>>> GetAvailableVouchers()
    {

        var result = await Mediator.Send(new GetAvailableVouchersQuery { });
        return Ok(result);
    }

    // 2. GET /api/vouchers/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<VoucherDetailDto>> GetVoucherDetail(Guid id)
    {
        var result = await Mediator.Send(new GetVoucherDetailQuery { Id = id });
        return Ok(result);
    }

    // 3. POST /api/vouchers/validate
    [HttpPost("validate")]
    [Authorize]
    public async Task<ActionResult<VoucherValidationResultDto>> ValidateVoucher([FromBody] VoucherValidationRequestDto request)
    {
        var userId = Guid.Parse(_currentUserService.UserId);
        var result = await Mediator.Send(new ValidateVoucherCommand
        {
            VoucherCode = request.VoucherCode,
            OrderTotal = request.OrderTotal,
            UserId = userId
        });
        return Ok(result);
    }

    [HttpPost("validate-for-order")]
    [Authorize]
    public async Task<ActionResult<VoucherValidationResultDto>> ValidateVoucherForOrder(
            [FromBody] ValidateVoucherForOrderDto request)
    {
        var userId = Guid.Parse(_currentUserService.UserId);

        // Kiểm tra xem đơn hàng có tồn tại và thuộc về người dùng hiện tại không
        var order = await Mediator.Send(new GetOrderByIdQuery(request.OrderId));
        if (order == null)
        {
            return NotFound("Order not found");
        }

        if (order.CustomerId != userId)
        {
            return Forbid("You don't have permission to access this order");
        }

        var result = await Mediator.Send(new ValidateVoucherCommand
        {
            VoucherCode = request.VoucherCode,
            OrderId = request.OrderId,
            UserId = userId
        });

        return Ok(result);
    }

}
