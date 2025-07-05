using SportManager.Domain.Entity;

namespace SportManager.Application.Orders.Models;

public class PlaceOrderCommand : IRequest<Guid>
{
    public string? Notes { get; set; }
    public Guid ShippingAddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    // ✅ Người dùng chọn sản phẩm nào, số lượng bao nhiêu
    public List<OrderItemRequest> Items { get; set; } = [];
    public string? VoucherCode { get; set; }
}

public class OrderItemRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class ShippingAddressViewOrder
{
    public string ReceiveName { get; set; }
    public string Phone { get; set; }
    public string AddressDetail { get; set; }
}
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? Notes { get; set; }
    public StateOrder State { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public DateTime? PreparingDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CanceledDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? ReasonCancel { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? VoucherCode { get; set; }
    public decimal Total { get; set; }
    public PaymentOderDto Payment { get; set; }
    public ShippingAddressViewOrder ShippingAddress { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}

public class PaymentOderDto
{
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}