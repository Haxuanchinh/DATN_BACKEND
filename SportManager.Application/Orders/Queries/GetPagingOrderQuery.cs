using SportManager.Application.Common.Interfaces;
using SportManager.Application.Orders.Models;
using SportManager.Application.Common.Exception;
using Microsoft.IdentityModel.Tokens;
using SportManager.Domain.Entity;

namespace SportManager.Application.Orders.Queries;

public class GetOrdersWithPaginationQuery : IRequest<PageResult<OrderDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? KeyWord { get; set; }
    public StateOrder? State { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? CustomerId { get; set; }
}

public class GetOrdersWithPaginationQueryHandler : IRequestHandler<GetOrdersWithPaginationQuery, PageResult<OrderDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrdersWithPaginationQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<OrderDto>> Handle(GetOrdersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .Include(o => o.Voucher)
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .AsQueryable();

        if (!request.CustomerId.IsNullOrEmpty())
        {
            query = query.Where(o => o.CustomerId == Guid.Parse(request.CustomerId));
        }
        // Áp dụng các điều kiện lọc
        if (!string.IsNullOrEmpty(request.KeyWord))
        {
            query = query.Where(o =>
                o.Customer.User.Username.Contains(request.KeyWord) ||
                o.Id.ToString().Contains(request.KeyWord) ||
                o.Voucher.Code.Contains(request.KeyWord));
        }

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

        // Sắp xếp mặc định theo ngày đặt hàng giảm dần
        query = query.OrderByDescending(o => o.OrderDate);

        // Tạo query cho OrderDto
        var orderDtoQuery = query.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.User.Username,
            OrderDate = o.OrderDate,
            State = o.State,
            Notes = o.Notes,
            SubTotal = o.CalculateSubTotal(),
            DiscountAmount = o.DiscountAmount,
            Total = o.CalculateSubTotal() - o.DiscountAmount,
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
            request.PageNumber,
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