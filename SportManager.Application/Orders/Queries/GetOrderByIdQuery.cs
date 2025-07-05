using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Orders.Models;

namespace SportManager.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>
{
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Truy xuất đơn hàng trước
        var order = await _dbContext.Orders
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(o => o.ProductVariant)
            .Include(o => o.Voucher)
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
            return null;

        // Truy xuất địa chỉ giao hàng dựa trên ShippingAddressId từ đơn hàng
        var shippingAddress = await _dbContext.ShippingAddresses
            .FirstOrDefaultAsync(a => a.Id == order.ShippingAddressId, cancellationToken);

        // Tạo đối tượng ShippingAddressViewOrder nếu địa chỉ giao hàng tồn tại
        ShippingAddressViewOrder shippingAddressDto = null;
        if (shippingAddress != null)
        {
            shippingAddressDto = new ShippingAddressViewOrder
            {
                AddressDetail = $"{shippingAddress.Ward}, {shippingAddress.District}, {shippingAddress.City}, {shippingAddress.Country}",
                Phone = shippingAddress.Phone,
                ReceiveName = shippingAddress.RecipientName
            };
        }

        // Trả về OrderDto với thông tin địa chỉ giao hàng
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.User.Username,
            OrderDate = order.OrderDate,
            State = order.State,
            CanceledDate = order.CanceledDate,
            ConfirmedDate = order.ConfirmedDate,
            DeliveredDate = order.DeliveredDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            PreparingDate = order.PreparingDate,
            ReasonCancel = order.ReasonCancel,
            ShippedDate = order.ShippedDate,
            Notes = order.Notes,
            SubTotal = order.CalculateSubTotal(),
            DiscountAmount = order.DiscountAmount,
            Total = order.CalculateSubTotal() - order.DiscountAmount,
            Payment = order.Payment != null ? new PaymentOderDto
            {
                Method = order.Payment.Method,
                Status = order.Payment.Status,
                PaidAt = order.Payment.PaidAt
            } : null,
            VoucherCode = order.Voucher != null ? order.Voucher?.Code : null,
            ShippingAddress = shippingAddress != null
            ? new ShippingAddressViewOrder
            {
                AddressDetail = $"{shippingAddress.Ward}, {shippingAddress.District}, " +
                              $"{shippingAddress.City}, {shippingAddress.Country}",
                Phone = shippingAddress.Phone,
                ReceiveName = shippingAddress.RecipientName
            }
            : null,
            OrderItems = order.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductVariantId,
                ProductName = item.ProductVariant.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                ImageUrl = item.ProductVariant.Images?.FirstOrDefault()
            }).ToList()
        };
    }

}
