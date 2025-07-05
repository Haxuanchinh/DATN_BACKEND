using SportManager.Application.Abstractions;
using SportManager.Application.Carts.Models;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Carts.Queries;

public class GetCartQuery : IRequest<List<CartItemDto>> { }

public class GetCartQueryHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<GetCartQuery, List<CartItemDto>>
{
    public async Task<List<CartItemDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId.ToString() == _currentUser.UserId, cancellationToken);

        if (cart == null)
            return [];

        return cart.Items.Select(item => new CartItemDto
        {
            Id = item.Id,
            ProductVariantId = item.ProductVariantId,
            Quantity = item.Quantity,
            ProductVariant = item.ProductVariant != null ?
            new ProductVariantView
            {
                Id = item.ProductVariant.Id,
                StockQuantity = item.ProductVariant.StockQuantity,
                Attribute = item.ProductVariant.Attribute,
                Name = item.ProductVariant.Name,
                Images = item.ProductVariant.Images,
                SKU = item.ProductVariant.SKU,
                Price = item.ProductVariant.Price,
            } : null,
        }).ToList();
    }
}
