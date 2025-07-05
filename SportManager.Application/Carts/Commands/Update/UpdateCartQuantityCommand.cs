using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Carts.Commands.Update;

public class UpdateCartQuantityCommand : IRequest<bool>
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}

internal class UpdateCartQtyValidator : AbstractValidator<UpdateCartQuantityCommand>
{
    public UpdateCartQtyValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("CartItemId không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.")
            .LessThanOrEqualTo(100).WithMessage("Số lượng không được vượt quá 100.");
    }
}

public class UpdateCartQtyCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<UpdateCartQuantityCommand, bool>
{
    public async Task<bool> Handle(UpdateCartQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(c => c.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId.ToString() == _currentUser.UserId, cancellationToken);

        if (cart == null)
            throw new ApplicationException("Không tìm thấy giỏ hàng.");

        var findCartItem = cart.Items.FirstOrDefault(x => x.Id == request.CartItemId);
        if (findCartItem == null)
            throw new ApplicationException("Không tìm thấy sản phẩm trong giỏ hàng.");

        var stockQtyVariant = findCartItem.ProductVariant.StockQuantity;
        if (request.Quantity > stockQtyVariant)
            throw new ApplicationException("Vượt quá số lượng hàng trong kho!.");

        if (request.Quantity == 0)
        {
            _dbContext.CartItems.Remove(findCartItem);
        }
        else
        {
            findCartItem.Quantity = request.Quantity;
            _dbContext.CartItems.Update(findCartItem);
        }

        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
