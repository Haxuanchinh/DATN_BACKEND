using SportManager.Application.Abstractions;
using SportManager.Application.Carts.Models;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;

namespace SportManager.Application.Carts.Commands.Create;

public class AddToCartResponse
{
    public int totalCartItems { get; set; }
}
public class AddToCartCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<AddToCartCommand, AddToCartResponse>
{
    public async Task<AddToCartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            throw new ApplicationException("Số lượng sản phẩm phải lớn hơn 0.");

        var userId = Guid.Parse(_currentUser.UserId);

        // Lấy cart hiện tại của user
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>()
            };
            await _dbContext.Carts.AddAsync(cart, cancellationToken);
        }

        // Lấy variant từ DB
        var productVariant = await _dbContext.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId, cancellationToken);

        if (productVariant == null)
            throw new ApplicationException("Sản phẩm không tồn tại.");

        var existingItem = cart.Items
            .FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);

        var quantityInCart = existingItem?.Quantity ?? 0;
        var totalRequested = quantityInCart + request.Quantity;

        if (totalRequested > productVariant.StockQuantity)
        {
            var remainingStock = productVariant.StockQuantity - quantityInCart;
            if (remainingStock <= 0)
                throw new ApplicationException("Không thể thêm vào giỏ hàng vì số lượng bạn đã đặt đạt giới hạn tồn kho trong giỏ hàng.");

            request.Quantity = remainingStock;
        }

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UnitPrice = (int)productVariant.Price,
                Unit = 1,
            });
        }


        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AddToCartResponse
        {
            totalCartItems = cart.Items.Count()
        };

    }
}
