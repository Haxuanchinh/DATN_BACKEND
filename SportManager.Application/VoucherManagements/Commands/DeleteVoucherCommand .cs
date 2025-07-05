using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.VoucherManagements.Commands;

public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { request.Id }, cancellationToken);

        if (voucher == null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        _context.Vouchers.Remove(voucher);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value; // ✅ Quan trọng
    }
}


