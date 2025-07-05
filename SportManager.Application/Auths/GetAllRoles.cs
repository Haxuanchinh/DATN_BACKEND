using SportManager.Application.Common.Interfaces;
using SportManager.Application.Users.Models;

namespace SportManager.Application.Auths;

public class GetAllRoles : IRequest<PageResult<RoleView>>
{
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}

public class GetAllRolesHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetAllRoles, PageResult<RoleView>>
{
    public async Task<PageResult<RoleView>> Handle(GetAllRoles request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = dbContext.Roles
            .AsNoTracking()
            .Select(role => new RoleView
            {
                Id = role.Id,
                Name = role.Name,
            });

        return await PageResult<RoleView>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
