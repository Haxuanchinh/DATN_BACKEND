//using SportManager.Application.Common.Interfaces;

//namespace SportManager.Application.Suppliers.Cammands;


//public class DeleteSupplierCommand : IRequest<Unit>
//{
//    public Guid Id { get; set; }
//}

//public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
//{
//    private readonly IApplicationDbContext _dbContext;

//    public DeleteSupplierCommandHandler(IApplicationDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
//    {
//        cancellationToken.ThrowIfCancellationRequested();

//        var supplier = await _dbContext.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);
//        if (supplier == null)
//            throw new KeyNotFoundException("SUPPLIER_NOT_FOUND");

//        _dbContext.Suppliers.Remove(supplier);
//        await _dbContext.SaveChangesAsync(cancellationToken);

//        return Unit.Value;
//    }
//}