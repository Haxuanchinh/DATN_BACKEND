using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Payment)]
public class Payment : EntityBase<Guid>
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}

public enum PaymentMethod
{
    CashOnDelivery,//COD
    CreditCard, // thẻ tín dụng
    EWallet // ví điện tử
}

public enum PaymentStatus
{
    Pending, // Chờ thanh toán
    Completed, // Đã thanh toán
    Failed, // Thất bại thanh toán
    Refunded // Hoàn tiền
}