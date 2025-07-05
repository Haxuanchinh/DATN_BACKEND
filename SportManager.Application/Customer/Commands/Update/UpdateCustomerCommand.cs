using SportManager.Application.Abstractions;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Customer.Models;
using SportManager.Domain.Entity;
using Microsoft.EntityFrameworkCore; // Thêm namespace này cho FirstOrDefaultAsync

namespace SportManager.Application.Customer.Commands.Update;

public class UpdateCustomerCommand : CustomerDto, IRequest<UpdateCustomerResponse>
{
    public required Guid Id { get; set; }
    public string? ConfirmPassWord { get; set; } = null;

    public void Normalize()
    {
        // Kiểm tra null trước khi Trim() để tránh lỗi NullReferenceException
        if (Address != null)
        {
            Address = Address.Trim();
        }
        if (Phone != null)
        {
            Phone = Phone.Trim();
        }
    }
}

public record UpdateCustomerResponse
{
    public Guid Id { get; set; }
}

public class UpdateCustomerValidator : CustomerValidatorBase<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("CUSTOMER_ID_REQUIRED");

        ValidateFormatAndLength();
        ValidateShippingAddresses(true);
    }
}

public class UpdateCustomerCommandHandler(IApplicationDbContext applicationDbContext, IAuthService _passwordHasher)
    : IRequestHandler<UpdateCustomerCommand, UpdateCustomerResponse>
{
    public async Task<UpdateCustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var customer = await applicationDbContext.Customers
            .Include(c => c.ShippingAddresses)
            .Include(c => c.User)
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        // Cập nhật thông tin customer
        customer.Address = request.Address;
        customer.Age = request.Age;
        customer.Gender = (Gender)request.Gender;

        // Xử lý thông tin User liên quan
        // Chỉ cập nhật Avatar nếu có giá trị mới
        if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            customer.User.Avatar = request.AvatarUrl;
        }

        // Chỉ cập nhật Email nếu có giá trị mới
        // Lưu ý: Cột Email thường cũng là NOT NULL và UNIQUE.
        // Bạn có thể cần thêm logic kiểm tra trùng lặp Email nếu cần.
        if (!string.IsNullOrEmpty(request.Email))
        {
            customer.User.Email = request.Email;
        }

        // --- ĐÂY LÀ PHẦN QUAN TRỌNG ĐỂ KHẮC PHỤC LỖI USERNAME ---
        // Chỉ cập nhật Username nếu request.UserName không null hoặc không rỗng.
        // Nếu request.UserName rỗng, chúng ta sẽ giữ lại Username hiện tại của người dùng
        // vì cột Username là NOT NULL.
        if (!string.IsNullOrEmpty(request.UserName))
        {
            customer.User.Username = request.UserName;
        }
        // --- KẾT THÚC PHẦN KHẮC PHỤC LỖI USERNAME ---

        // Cập nhật mật khẩu chỉ khi cả Password và ConfirmPassWord được cung cấp và khớp
        if (!string.IsNullOrEmpty(request.Password) && request.Password == request.ConfirmPassWord)
        {
            // Sử dụng Username hiện tại của user để hash mật khẩu,
            // đảm bảo tính nhất quán ngay cả khi Username không được cập nhật từ request.
            customer.User.PasswordHash = _passwordHasher.HashPassword(request.Password, customer.User.Username);
        }

        // Xử lý ShippingAddresses
        if (request.ShippingAddresses is not null)
        {
            var inputMap = request.ShippingAddresses
                .Where(x => x.Id != null && x.Id != Guid.Empty)
                .ToDictionary(x => x.Id!.Value);

            var existingMap = customer.ShippingAddresses.ToDictionary(x => x.Id);

            var updatedAddresses = request.ShippingAddresses.Select(input =>
            {
                if (input.Id != null && input.Id != Guid.Empty && existingMap.TryGetValue(input.Id.Value, out var existing))
                {
                    return MapShippingAddress(input, existing);
                }

                return MapShippingAddress(input);
            }).ToList();

            // Tìm và xoá các address bị remove
            var removedIds = existingMap.Keys.Except(inputMap.Keys).ToList();
            if (removedIds.Any())
            {
                var addressesToRemove = customer.ShippingAddresses
                    .Where(x => removedIds.Contains(x.Id))
                    .ToList();

                applicationDbContext.ShippingAddresses.RemoveRange(addressesToRemove);
            }

            customer.ShippingAddresses = updatedAddresses;
        }

        applicationDbContext.Customers.Update(customer);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerResponse { Id = customer.Id };
    }

    private static ShippingAddress MapShippingAddress(ShippingAddressView dto, ShippingAddress? existing = null)
    {
        var address = existing ?? new ShippingAddress();

        address.AddressLine = dto.AddressLine;
        address.Ward = dto.Ward;
        address.City = dto.City;
        address.CityId = dto.CityId;
        address.Country = dto.Country;
        address.CountryId = dto.CountryId;
        address.District = dto.District;
        address.Phone = dto.Phone?.Trim(); // Sử dụng ?.Trim() để xử lý null an toàn
        address.PostalCode = dto.PostalCode;
        address.RecipientName = dto.RecipientName;
        address.IsDefault = dto.IsDefault;

        return address;
    }
}