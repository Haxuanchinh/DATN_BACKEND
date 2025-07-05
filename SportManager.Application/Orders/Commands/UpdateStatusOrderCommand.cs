using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;

namespace SportManager.Application.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public StateOrder NewStatus { get; set; }
    public string? Reason { get; set; }
    public Guid? ShipperId { get; set; }
    public string? ImageConfirmed { get; set; }
}

public class UpdateOrderStatusCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser,
    IPushNotificationService _pushNotificationService)
    : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ApplicationException("Đơn hàng không tồn tại.");
        }

        var currentUserId = Guid.Parse(_currentUser.UserId);
        var isAdmin = _currentUser.Roles.Contains("Admin");
        var isShipper = _currentUser.Roles.Contains("Shipper");

        ValidateStatusTransition(order.State, request.NewStatus, isAdmin, isShipper);

        StateOrder oldStatus = order.State;

        string notificationTitle = "";
        string notificationBody = "";
        Dictionary<string, string> notificationData = new()
        {
            { "orderId", order.Id.ToString() },
            { "status", request.NewStatus.ToString().ToLower() } // Add status to data payload
        };

        switch (request.NewStatus)
        {
            case StateOrder.Canceled:
                await HandleCancelOrder(order, request.Reason, isAdmin, cancellationToken);
                notificationTitle = "Đơn hàng của bạn đã bị hủy!";
                notificationBody = $"Mã đơn hàng: #{order.Id}. Lý do: {order.ReasonCancel ?? "Không có lý do cụ thể."}";
                break;

            case StateOrder.Confirmed:
                order.State = StateOrder.Confirmed;
                order.ConfirmedDate = DateTime.UtcNow;
                notificationTitle = "Đơn hàng của bạn đã được xác nhận!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được cửa hàng xác nhận và đang chuẩn bị.";
                break;

            case StateOrder.Processing:
                order.State = StateOrder.Processing;
                order.PreparingDate = DateTime.UtcNow;
                notificationTitle = "Đơn hàng của bạn đang được xử lý!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đang được chuẩn bị bởi cửa hàng.";
                break;

            case StateOrder.Shipped:
                if (request.ShipperId == null && isShipper)
                {
                    request.ShipperId = currentUserId;
                }
                order.State = StateOrder.Shipped;
                order.ShippedDate = DateTime.UtcNow;
                order.ShipperId = request.ShipperId;
                order.ImageConfirmed = request.ImageConfirmed;
                notificationTitle = "Đơn hàng của bạn đang được giao!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đang trên đường đến bạn.";
                break;

            case StateOrder.Delivered:
                order.State = StateOrder.Delivered;
                order.DeliveredDate = DateTime.UtcNow;
                order.Payment.Status = PaymentStatus.Completed;
                notificationTitle = "Đơn hàng của bạn đã được giao thành công!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được giao. Cảm ơn bạn đã mua sắm.";
                break;

            case StateOrder.Receivered: // This state seems to transition back to Pending in ValidateStatusTransition, re-evaluate its purpose
                order.State = StateOrder.Delivered; // Keep this consistent with the earlier example, though typically Receivered might imply a sub-state before final delivery.
                                                    // If 'Receivered' means 'received by internal system for processing', it might be better handled differently.
                notificationTitle = "Đơn hàng đã được tiếp nhận!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được tiếp nhận. Chúng tôi sẽ xử lý sớm.";
                break;

            case StateOrder.Sendered: // Similar to Receivered, re-evaluate its exact meaning and flow
                order.State = StateOrder.Sendered;
                notificationTitle = "Đơn hàng đã được gửi đi!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được gửi từ kho.";
                break;

            case StateOrder.Returned: // Assuming this is a valid transition that should also notify
                order.State = StateOrder.Returned;
                notificationTitle = "Đơn hàng của bạn đã được trả lại!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được trả lại. Vui lòng liên hệ hỗ trợ nếu bạn có thắc mắc.";
                break;

            case StateOrder.Refunded: // Assuming this is a valid transition that should also notify
                order.State = StateOrder.Refunded;
                notificationTitle = "Đơn hàng của bạn đã được hoàn tiền!";
                notificationBody = $"Mã đơn hàng: #{order.Id} đã được hoàn tiền. Vui lòng kiểm tra tài khoản của bạn.";
                break;
        }

        // Handle specific case for RejectCancel (which wasn't in your switch, but in your previous if-block)
        // This implicitly changes the state *after* the switch
        if (request.NewStatus == StateOrder.RejectCancel)
        {
            // You had 'order.State = StateOrder.Shipped;' here. 
            // This would override the state set in the switch if NewStatus was already Shippered.
            // Consider if RejectCancel is truly a separate status or just a flag on Canceled.
            // For now, I'll assume you want to reset it to Shippered and notify.
            order.State = StateOrder.Shipped; // Reset state if cancellation was rejected
            order.ReasonRejectCancel = request.Reason;
            notificationTitle = "Yêu cầu hủy đơn hàng của bạn đã bị từ chối!";
            notificationBody = $"Mã đơn hàng: #{order.Id}. Yêu cầu hủy đã bị từ chối. Lý do: {order.ReasonRejectCancel} Đơn hàng đang ở trạng thái: {order.State}.";
        }


        foreach (var item in order.OrderItems)
        {
            item.State = order.State;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send push notification after saving changes, if the customer exists
        if (order.Customer?.User?.Id != null && !string.IsNullOrEmpty(notificationTitle))
        {
            await _pushNotificationService.SendNotificationToUserAsync(
                order.Customer.User.Id.ToString(),
                notificationTitle,
                notificationBody,
                notificationData
            );
        }

        return true;
    }

    private async Task HandleCancelOrder(Order order, string? reason, bool isAdmin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(reason) && !isAdmin)
        {
            throw new ApplicationException("Vui lòng cung cấp lý do hủy đơn hàng.");
        }

        order.State = StateOrder.Canceled;
        order.ReasonCancel = reason;
        order.CanceledDate = DateTime.UtcNow;

        var orderItems = await _dbContext.OrderItems
            .Include(oi => oi.ProductVariant)
            .Where(oi => oi.OrderId == order.Id)
            .ToListAsync(cancellationToken);

        foreach (var item in orderItems)
        {
            item.ProductVariant.StockQuantity += item.Quantity;
        }

        // Important: You had PaymentMethod.CashOnDelivery here.
        // It should be PaymentStatus.Pending (or relevant pre-completion status) if you want to refund only unpaid COD orders upon cancellation.
        // Or if it's already Completed and you're refunding, then PaymentStatus.Completed makes sense.
        // I'm assuming you meant to check the current payment status before marking as refunded.
        if (order.Payment != null && order.Payment.Status == PaymentStatus.Completed)
        {
            order.Payment.Status = PaymentStatus.Refunded;
        }
    }

    private void ValidateStatusTransition(StateOrder currentStatus, StateOrder newStatus, bool isAdmin, bool isShipper)
    {
        // Define all valid transitions
        var validTransitions = new Dictionary<StateOrder, List<StateOrder>>()
        {
            [StateOrder.Pending] = new() { StateOrder.Confirmed, StateOrder.Canceled, StateOrder.Processing, StateOrder.RejectCancel },
            [StateOrder.Confirmed] = new() { StateOrder.Processing, StateOrder.Canceled, StateOrder.RejectCancel },
            [StateOrder.Processing] = new() { StateOrder.Confirmed, StateOrder.Shipped, StateOrder.Canceled, StateOrder.RejectCancel }, // Added Canceled from Processing
            [StateOrder.Shipped] = new() { StateOrder.Delivered, StateOrder.Canceled, StateOrder.Returned },
            [StateOrder.Delivered] = new() { StateOrder.Returned }, // Can a delivered order be returned? If so, this is fine.
            [StateOrder.RequestCancel] = new() { StateOrder.RejectCancel, StateOrder.Canceled },
            [StateOrder.Returned] = new() { StateOrder.Refunded },
            [StateOrder.Refunded] = new() { },
            [StateOrder.Receivered] = new() { StateOrder.Pending }, // Re-evaluate: This seems like an initial internal state.
            [StateOrder.Sendered] = new() { StateOrder.Pending } // Re-evaluate: This seems like an initial internal state.
        };

        // Add `RejectCancel` to validTransitions if it's a distinct status
        if (!validTransitions.ContainsKey(StateOrder.RejectCancel))
        {
            validTransitions.Add(StateOrder.RejectCancel, new() { StateOrder.Shipped, StateOrder.Processing }); // After rejection, where does it go?
        }


        // Shipper specific transitions
        if (isShipper)
        {
            if (currentStatus == StateOrder.Shipped && newStatus == StateOrder.Delivered)
            {
                return; // Valid for shipper
            }
            if (currentStatus == StateOrder.Shipped && newStatus == StateOrder.Returned)
            {
                return; // Shipper might also mark as returned
            }
            throw new ApplicationException("Shipper chỉ có thể cập nhật trạng thái đơn hàng Đang giao sang Đã giao hàng hoặc Đã trả lại.");
        }

        // Admin can cancel from any state (handled implicitly by the case below, but explicitly added for clarity)
        if (isAdmin && newStatus == StateOrder.Canceled)
        {
            return;
        }

        // Admin can reject cancellation
        if (isAdmin && currentStatus == StateOrder.Canceled && newStatus == StateOrder.RejectCancel)
        {
            return;
        }

        // General transition validation
        if (!validTransitions.TryGetValue(currentStatus, out var allowedTransitions) || !allowedTransitions.Contains(newStatus))
        {
            throw new ApplicationException($"Không thể chuyển từ trạng thái {currentStatus} sang {newStatus}.");
        }
    }
}