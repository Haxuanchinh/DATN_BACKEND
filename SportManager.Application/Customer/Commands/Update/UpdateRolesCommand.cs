using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;

namespace SportManager.Application.Customer.Commands.Update;
public class UpdateRolesCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public List<Guid> RoleIds { get; set; }
}

public class UpdateRolesCommandHandler : IRequestHandler<UpdateRolesCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateRolesCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, request.UserId.ToString());

        // Xóa các quyền cũ
        _dbContext.UserRoles.RemoveRange(user.UserRoles);

        // Thêm các quyền mới
        foreach (var roleId in request.RoleIds.Distinct())
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
