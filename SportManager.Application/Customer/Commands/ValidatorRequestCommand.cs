using SportManager.Application.Common.Exception;
using SportManager.Application.Customer.Models;

public abstract class CustomerValidatorBase<T> : AbstractValidator<T> where T : CustomerDto
{
    protected void ValidateFormatAndLength()
    {
        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage(ErrorCode.MAX_LENGTH_200);

        RuleFor(x => x.Phone)
            .Matches(@"^\d{9,12}$").WithMessage("INVALID_PHONE");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("INVALID_EMAIL");

        RuleFor(x => x.UserName)
            .MinimumLength(3).WithMessage("MIN_LENGTH_3");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("MIN_LENGTH_6");
    }

    protected void ValidateShippingAddresses(bool isUpdate)
    {
        RuleForEach(x => x.ShippingAddresses)
            .SetValidator(new ShippingAddressValidator(isUpdate))
            .When(x => x.ShippingAddresses != null);
    }
}

public class ShippingAddressValidator : AbstractValidator<ShippingAddressView>
{
    public ShippingAddressValidator(bool isUpdate)
    {
        if (isUpdate)
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("SHIPPING_ADDRESS_ID_REQUIRED");
        }

        RuleFor(x => x.AddressLine)
            .MaximumLength(200).WithMessage(ErrorCode.MAX_LENGTH_200);
        RuleFor(x => x.Phone)
            .Matches(@"^\d{9,12}$").WithMessage("INVALID_PHONE");

        if (!isUpdate)
        {
            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);
            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);
        }
    }
}
