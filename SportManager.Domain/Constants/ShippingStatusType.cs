namespace SportManager.Domain.Constants;

public enum ShippingStatus
{
    Pending,        // Chờ lấy hàng
    PickedUp,       // Đã lấy hàng
    InTransit,      // Đang vận chuyển
    OutForDelivery, // Đang giao hàng
    Delivered,      // Đã giao thành công
    Failed,         // Giao thất bại
    Returned        // Đã trả lại
}