using SportManager.Application.Abstractions;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Orders.Models;
using SportManager.Application.Utilities;
using SportManager.Domain.Entity;

namespace SportManager.Application.Orders.Queries;

public class GetCustomerOrdersQuery : IRequest<PageResult<OrderDto>>
{
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string? KeyWord { get; set; }
    public StateOrder? State { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, PageResult<OrderDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerOrdersQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PageResult<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        // Lấy CustomerId từ thông tin user đăng nhập
        var customerId = _currentUserService.CustomerId;
        if (customerId == null)
        {
            throw new UnauthorizedAccessException("User is not a customer");
        }

        var query = _dbContext.Orders
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .Include(o => o.Voucher)
            .Where(o => o.CustomerId == Guid.Parse(customerId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.KeyWord))
        {
            query = query.Where(o =>
                o.Customer.User.Username.Contains(request.KeyWord) ||
                o.Id.ToString().Contains(request.KeyWord) ||
                o.Voucher.Code.Contains(request.KeyWord));
        }

        // Các điều kiện lọc khác
        if (request.State.HasValue)
        {
            query = query.Where(o => o.State == request.State);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= request.ToDate.Value);
        }

        query = query.OrderByDescending(o => o.OrderDate);

        var orderDtoQuery = query.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.User.Username,
            OrderDate = o.OrderDate,
            State = o.State,
            Notes = o.Notes,
            SubTotal = o.CalculateSubTotal(),
            CanceledDate = o.CanceledDate,
            ConfirmedDate = o.ConfirmedDate,
            DeliveredDate = o.DeliveredDate,
            ExpectedDeliveryDate = o.ExpectedDeliveryDate,
            PreparingDate = o.PreparingDate,
            ReasonCancel = o.ReasonCancel,
            ShippedDate = o.ShippedDate,
            DiscountAmount = o.DiscountAmount,
            Total = o.CalculateSubTotal() - o.DiscountAmount,
            Payment = o.Payment != null ? new PaymentOderDto
            {
                Method = o.Payment.Method,
                Status = o.Payment.Status,
                PaidAt = o.Payment.PaidAt
            } : null,
            VoucherCode = o.Voucher != null ? o.Voucher.Code : null,
            OrderItems = o.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductVariantId,
                ProductName = item.ProductVariant.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        });

        // Thực hiện phân trang
        var result = await PageResult<OrderDto>.CreateAsync(
            orderDtoQuery,
            request.PageNumber, // Trừ 1 vì pageNumber thường bắt đầu từ 1 trên UI
            request.PageSize,
            cancellationToken);

        // Bổ sung thông tin địa chỉ giao hàng cho từng đơn hàng
        foreach (var orderDto in result.Items)
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderDto.Id, cancellationToken);

            if (order?.ShippingAddressId != null)
            {
                var shippingAddress = await _dbContext.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.Id == order.ShippingAddressId, cancellationToken);

                if (shippingAddress != null)
                {
                    orderDto.ShippingAddress = new ShippingAddressViewOrder
                    {
                        AddressDetail = $"{shippingAddress.Ward}, {shippingAddress.District}, {shippingAddress.City}, {shippingAddress.Country}",
                        Phone = shippingAddress.Phone,
                        ReceiveName = shippingAddress.RecipientName
                    };
                }
            }
        }

        return result;
    }
}