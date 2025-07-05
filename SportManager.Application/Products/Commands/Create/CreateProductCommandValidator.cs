using SportManager.Application.Common.Exception;

namespace SportManager.Application.Products.Commands.Create;

internal class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        // Validate Product
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED)
            .MaximumLength(100).WithMessage("PRODUCT_NAME_MAX_LENGTH_100");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.Variants)
            .NotNull().WithMessage("VARIANT_LIST_REQUIRED")
            .Must(x => x.Any()).WithMessage("AT_LEAST_ONE_VARIANT_REQUIRED");

        // Validate từng Variant
        RuleForEach(x => x.Variants).ChildRules(variant =>
        {
            variant.RuleFor(v => v.Name)
                .NotEmpty().WithMessage("VARIANT_NAME_REQUIRED");

            variant.RuleFor(v => v.SKU)
                .NotEmpty().WithMessage("VARIANT_SKU_REQUIRED");

            variant.RuleFor(v => v.Color)
                .NotEmpty().WithMessage("VARIANT_COLOR_REQUIRED");

            variant.RuleFor(v => v.Size)
                .NotEmpty().WithMessage("VARIANT_SIZE_REQUIRED");

            variant.RuleFor(v => v.Unit)
                .NotEmpty().WithMessage("VARIANT_UNIT_REQUIRED");

            variant.RuleFor(v => v.Price)
                .GreaterThan(0).WithMessage("VARIANT_PRICE_MUST_BE_GREATER_THAN_ZERO");

            variant.RuleFor(v => v.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("VARIANT_STOCK_MUST_BE_NON_NEGATIVE");
        });

        // Validate ProductCategories nếu có
        When(x => x.ProductCategories != null && x.ProductCategories.Any(), () =>
        {
            RuleForEach(x => x.ProductCategories).ChildRules(cat =>
            {
                // Bắt buộc phải có Category.Id hoặc thông tin đầy đủ để tạo mới
                cat.RuleFor(c => c)
                    .Must(c => c.Id.HasValue || c.Category != null && !string.IsNullOrWhiteSpace(c.Category.Name))
                    .WithMessage("CATEGORY_ID_OR_NAME_REQUIRED");

                // Nếu tạo mới Category thì validate Name
                cat.When(c => c.Category != null, () =>
                {
                    cat.RuleFor(c => c.Category!.Name)
                        .NotEmpty().WithMessage("CATEGORY_NAME_REQUIRED");
                });
            });
        });
    }
}
