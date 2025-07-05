using SportManager.Application.Abstractions;
using SportManager.Application.Carts.Models;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Carts.Commands.Update;

public class UpdateCartCommand : IRequest<bool>
{
    public List<CartItemDto> Items { get; set; } = [];
}

public class UpdateCartCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<UpdateCartCommand, bool>
{
    public async Task<bool> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            throw new ApplicationException("Giỏ hàng không có sản phẩm nào.");

        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId.ToString() == _currentUser.UserId, cancellationToken);

        if (cart == null)
            throw new ApplicationException("Không tìm thấy giỏ hàng.");

        cart.Items.Clear(); // Reset lại giỏ hàng

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ApplicationException("Số lượng sản phẩm phải lớn hơn 0.");

            var productVariant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == item.ProductVariantId, cancellationToken);

            if (productVariant == null)
                throw new ApplicationException("Sản phẩm không tồn tại.");

            if (item.Quantity > productVariant.StockQuantity)
                throw new ApplicationException($"Không thể cập nhật {item.Quantity} sản phẩm '{productVariant.Name}' vì tồn kho chỉ còn {productVariant.StockQuantity}.");

            cart.Items.Add(new Domain.Entity.CartItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = (int)productVariant.Price,
                Unit = 1
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
