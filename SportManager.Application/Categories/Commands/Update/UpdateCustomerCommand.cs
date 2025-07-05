using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;
using SportManager.Application.Categroies.Models;

namespace SportManager.Application.Category.Commands.Update;

public class UpdateCategoryCommand : CategoryDto, IRequest<UpdateCategoryResponse>
{
    public required Guid Id { get; set; }

}

public record UpdateCategoryResponse
{
    public Guid Id { get; set; }
}

public class UpdateCategoryCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customer = await applicationDbContext.Categories
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        // Cập nhật thông tin customer


        //customer.User.Email = request.Email;
        //customer.User.Username = request.UserName;

        customer.Logo = request.Logo;
        customer.Name = request.Name;
        customer.Description = request.Description;

        applicationDbContext.Categories.Update(customer);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResponse { Id = customer.Id };
    }

    //private async Task<bool> IsEmailOrUsernameExist(UpdateCategoryCommand request, CancellationToken cancellationToken)
    //{
    //    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    //    var normalizedUsername = request.UserName.Trim().ToLowerInvariant();

    //    return await applicationDbContext.Categorys
    //        .AnyAsync(x => x.Id != request.Id &&
    //                       (x.User.Email.Trim().ToLowerInvariant() == normalizedEmail ||
    //                        x.User.Username.Trim().ToLowerInvariant() == normalizedUsername),
    //            cancellationToken);
    //}


}
