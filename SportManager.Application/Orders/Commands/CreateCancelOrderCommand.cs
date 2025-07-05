using SportManager.Application.Common.Interfaces;
using SportManager.Application.Abstractions;
using SportManager.Domain.Entity;

namespace SportManager.Application.Orders.Commands;

public class RequestCancelOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
    public string? DetailReason { get; set; }
}

public class RequestCancelOrderCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser,
    IPushNotificationService _pushNotificationService)
    : IRequestHandler<RequestCancelOrderCommand, bool>
{
    public async Task<bool> Handle(RequestCancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ApplicationException("Đơn hàng không tồn tại.");
        }

        // Lấy CustomerId từ CurrentUserService, đảm bảo chuyển đổi đúng kiểu nếu cần
        if (!Guid.TryParse(_currentUser.CustomerId, out Guid requestingCustomerId))
        {
            throw new ApplicationException("Không thể xác định người dùng yêu cầu.");
        }

        if (order.CustomerId != requestingCustomerId)
        {
            throw new ApplicationException("Bạn không có quyền yêu cầu hủy đơn hàng này.");
        }

        if (order.State != StateOrder.Pending &&
            order.State != StateOrder.Confirmed &&
            order.State != StateOrder.Processing)
        {
            throw new ApplicationException($"Không thể yêu cầu hủy đơn hàng ở trạng thái {order.State}.");
        }

        order.State = StateOrder.RequestCancel;
        order.ReasonCancel = $"{request.Reason}{(string.IsNullOrEmpty(request.DetailReason) ? "" : $" ({request.DetailReason})")}";

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Lấy danh sách Admin
        var admins = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
            .ToListAsync(cancellationToken);

        // Gửi thông báo đến tất cả các thiết bị của mỗi Admin
        foreach (var admin in admins)
        {
            // Kiểm tra xem admin có FcmTokens nào không
            if (admin.FcmTokens != null && admin.FcmTokens.Any())
            {
                var title = "Yêu cầu hủy đơn hàng mới!";
                var customerInfo = order.Customer?.User?.Username ?? order.Customer?.User?.Email ?? order.Customer?.User?.Id.ToString();
                var body = $"Khách hàng {customerInfo} đã yêu cầu hủy đơn hàng #{order.Id}. Lý do: {order.ReasonCancel}";
                var data = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() },
                    { "action", "review_cancel_request" },
                    { "customerId", order.CustomerId.ToString() }
                };

                // Sử dụng SendNotificationToUserAsync để gửi đến tất cả các token của admin này
                await _pushNotificationService.SendNotificationToUserAsync(admin.Id.ToString(), title, body, data);
            }
            else
            {
                Console.WriteLine($"Admin {admin.Id} không có FCM token nào được đăng ký.");
            }
        }

        return true;
    }
}
