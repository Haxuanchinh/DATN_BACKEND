using SportManager.Application.Abstractions;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Orders.Models;
using SportManager.Domain.Entity;
using ValidateVoucherCommand = SportManager.Application.Common.Exception.ValidateVoucherCommand;

namespace SportManager.Application.Orders.Commands;

public class PlaceOrderCommandHandler(
    IApplicationDbContext _dbContext,
    IPushNotificationService _pushNotificationService,
    ICurrentUserService _currentUser,
    IMediator _mediator)
    : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUser.UserId);
        var customer = await _dbContext.Customers
        .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
        {
            throw new ApplicationException("Customer not found for the current user.");
        }

        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || !cart.Items.Any())
            throw new ApplicationException("Không có sản phẩm trong giỏ hàng.");

        var requestedItems = request.Items;
        if (!requestedItems.Any())
            throw new ApplicationException("Danh sách sản phẩm đặt hàng trống.");

        // Tạo đơn hàng
        var order = new Order
        {
            CustomerId = customer.Id,
            Notes = request.Notes,
            State = StateOrder.Pending,
            OrderDate = DateTime.UtcNow,
            ShippingAddressId = request.ShippingAddressId
        };

        var cartItemIdsToRemove = new List<Guid>();

        // Xử lý các item trong đơn hàng
        foreach (var reqItem in requestedItems)
        {
            var cartItem = cart.Items.FirstOrDefault(ci => ci.ProductVariantId == reqItem.ProductVariantId);
            if (cartItem == null)
                throw new ApplicationException("Sản phẩm đặt hàng không có trong giỏ.");

            if (reqItem.Quantity <= 0 || reqItem.Quantity > cartItem.Quantity)
                throw new ApplicationException($"Số lượng không hợp lệ cho sản phẩm {reqItem.ProductVariantId}.");

            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == reqItem.ProductVariantId, cancellationToken);

            if (variant == null)
                throw new ApplicationException("Sản phẩm không tồn tại.");

            if (variant.StockQuantity < reqItem.Quantity)
                throw new ApplicationException($"Sản phẩm '{variant.Name}' không đủ tồn kho. Chỉ còn {variant.StockQuantity}.");

            variant.StockQuantity -= reqItem.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                ProductVariantId = variant.Id,
                Quantity = reqItem.Quantity,
                UnitPrice = variant.Price,
                State = StateOrder.Pending
            });

            if (cartItem.Quantity == reqItem.Quantity)
                cartItemIdsToRemove.Add(cartItem.Id);
            else
                cartItem.Quantity -= reqItem.Quantity;
        }

        // Xử lý voucher nếu có
        if (!string.IsNullOrEmpty(request.VoucherCode))
        {
            decimal orderTotal = order.CalculateSubTotal();

            // Validate voucher
            var validationResult = await _mediator.Send(new ValidateVoucherCommand
            {
                VoucherCode = request.VoucherCode,
                OrderTotal = orderTotal,
                UserId = userId
            }, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new ApplicationException($"Voucher không hợp lệ: {validationResult.ErrorMessage}");
            }

            // Lấy thông tin voucher
            var voucher = await _dbContext.Vouchers
                .FirstOrDefaultAsync(v => v.Code == request.VoucherCode, cancellationToken);

            if (voucher != null)
            {
                // Tính discount amount
                decimal discountAmount = order.CalculateDiscountAmount(voucher);

                // Áp dụng voucher vào order
                order.VoucherId = voucher.Id;
                order.DiscountAmount = discountAmount;

                // Tạo bản ghi sử dụng voucher
                var voucherUsage = new VoucherUsage
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucher.Id,
                    UserId = userId,
                    OrderId = order.Id,
                    UsedAt = DateTime.UtcNow
                };
                _dbContext.VoucherUsages.Add(voucherUsage);
            }
        }

        // Tạo payment

        var payment = new Payment
        {
            Method = request.PaymentMethod,
            Status = request.PaymentMethod == PaymentMethod.CashOnDelivery ? PaymentStatus.Pending : PaymentStatus.Completed,
            Order = order
        };


        order.Payment = payment;

        _dbContext.Orders.Add(order);
        _dbContext.Payments.Add(payment);

        // Xóa cart items đã đặt hàng
        if (cartItemIdsToRemove.Any())
        {
            var removeItems = cart.Items.Where(i => cartItemIdsToRemove.Contains(i.Id));
            _dbContext.CartItems.RemoveRange(removeItems);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
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
                var title = "Đơn hàng mới!";
                var customerInfo = order.Customer?.User?.Username ?? order.Customer?.User?.Email ?? order.Customer?.User?.Id.ToString();
                var body = $"Khách hàng {customerInfo} đã tạo đơn đơn hàng #{order.Id}";
                var data = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() },
                    { "action", "create_order" },
                    { "customerId", order.CustomerId.ToString() }
                };

                await _pushNotificationService.SendNotificationToUserAsync(admin.Id.ToString(), title, body, data);
            }
            else
            {
                Console.WriteLine($"Admin {admin.Id} không có FCM token nào được đăng ký.");
            }
        }
        return order.Id;
    }
}