using SportManager.Application.AdminDashboards.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;


namespace SportManager.Application.AdminDashboards.Queries;

public class GetsChartAndRpQuery : IRequest<ModelDashboardAdminResponse>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetsChartAndRpQueryQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsChartAndRpQuery, ModelDashboardAdminResponse>
{
    public async Task<ModelDashboardAdminResponse> Handle(GetsChartAndRpQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.Now.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.Now;

        var totalRevenue = await GetTotalRevenue(startDate, endDate, cancellationToken);
        var totalNewOrder = await GetTotalNewOrders(startDate, endDate, cancellationToken);
        var totalNewCustomer = await GetTotalNewCustomers(startDate, endDate, cancellationToken);
        var totalVoucher = await GetTotalVouchers(startDate, endDate, cancellationToken);
        var totalGMV = await GetTotalDiscount(startDate, endDate, cancellationToken);
        var lineChartData = await GetLineChartData(startDate, endDate, cancellationToken);

        return new ModelDashboardAdminResponse
        {
            TotalRevenue = totalRevenue,
            TotalNewOrder = totalNewOrder,
            TotalNewCustomer = totalNewCustomer,
            TotalVoucher = totalVoucher,
            TotalGMV = totalGMV,
            LineChartData = lineChartData
        };
    }
    private async Task<decimal> GetTotalRevenue(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate.AddDays(1))
            .Where(o => o.State == StateOrder.Delivered)
            .ToListAsync(cancellationToken);

        return orders.Sum(o => o.CalculateSubTotal() - o.DiscountAmount);
    }

    // Similarly for GetTotalDiscount
    private async Task<decimal> GetTotalDiscount(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var discount = await dbContext.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Where(o => o.State == StateOrder.Delivered)
            .SumAsync(o => o.DiscountAmount, cancellationToken);

        return discount;
    }

    // And for GetTotalGMVAlternative
    private async Task<decimal> GetTotalGMVAlternative(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var gmv = await dbContext.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Where(o => o.State == StateOrder.Delivered)
            .SumAsync(o => o.Total + o.DiscountAmount, cancellationToken);

        return gmv;
    }

    private async Task<int> GetTotalNewOrders(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var orderCount = await dbContext.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        return orderCount;
    }

    private async Task<int> GetTotalNewCustomers(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var customerCount = await dbContext.Customers
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        return customerCount;
    }

    private async Task<int> GetTotalVouchers(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var voucherCount = await dbContext.Vouchers
            .Where(v => v.CreatedAt >= startDate && v.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        return voucherCount;
    }

    private async Task<List<LineChartData>> GetLineChartData(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var queryResult = await dbContext.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var lineChartData = queryResult.Select(x => new LineChartData
        {
            Name = x.Date.ToString("dd/MM/yyyy"),
            Orders = x.Count
        }).ToList();

        // Fill empty dates if needed
        if (!lineChartData.Any())
        {
            return Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Select(date => new LineChartData
                {
                    Name = date.ToString("dd/MM/yyyy"),
                    Orders = 0
                })
                .ToList();
        }

        return lineChartData;
    }
    // Thêm method để lấy dữ liệu biểu đồ GMV theo ngày nếu cần
    private async Task<List<LineChartData>> GetGMVLineChartData(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var gmvChartData = await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Where(o => o.State == StateOrder.Delivered)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new LineChartData
            {
                Name = g.Key.ToString("dd/MM/yyyy"),
                Orders = (int)g.Sum(o => o.OrderItems.Sum(item => item.TotalPrice) + o.DiscountAmount)
            })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return gmvChartData;
    }
}