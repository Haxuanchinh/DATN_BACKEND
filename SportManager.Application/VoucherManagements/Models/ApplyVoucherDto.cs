using SportManager.Domain.Constants;

namespace SportManager.Application.VoucherManagements.Models;

public class ApplyVoucherResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? VoucherName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CreateVoucherCommand : IRequest<Guid>
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DiscountType DiscountTypeDisplay { get; set; } = default!;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public int? RemainingUsage { get; set; }
    public int? RemainingUserUsage { get; set; }
}

public class UpdateVoucherCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DiscountType DiscountTypeDisplay { get; set; } = default!;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public int? RemainingUsage { get; set; }
    public int? RemainingUserUsage { get; set; }
}

public class DeleteVoucherCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}


public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string DiscountTypeDisplay { get; set; } = default!;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public int? RemainingUsage { get; set; }
    public int? RemainingUserUsage { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class VoucherDetailDto : VoucherDto
{
    public int TotalUsage { get; set; }
    public int UserUsage { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class VoucherValidationRequestDto
{
    public string VoucherCode { get; set; } = default!;
    public decimal OrderTotal { get; set; }
}

public class VoucherValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public VoucherDto? Voucher { get; set; }
    public decimal DiscountAmount { get; set; }
}

//public class ApplyVoucherRequestDto
//{
//    public string VoucherCode { get; set; } = default!;
//    public Guid OrderId { get; set; }
//    public decimal OrderTotal { get; set; }
//}

public class AssignVoucherRequestDto
{
    public Guid VoucherId { get; set; }
    public Guid UserId { get; set; }
}

public class VoucherUsageDto
{
    public Guid Id { get; set; }
    public Guid VoucherId { get; set; }
    public string VoucherCode { get; set; } = default!;
    public string VoucherName { get; set; } = default!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }
}

public class ValidateVoucherForOrderDto
{
    public string VoucherCode { get; set; } = default!;
    public Guid OrderId { get; set; }
}

// Cập nhật DTO của Apply Voucher
public class ApplyVoucherRequestDto
{
    public string VoucherCode { get; set; } = default!;
    public Guid OrderId { get; set; }
}