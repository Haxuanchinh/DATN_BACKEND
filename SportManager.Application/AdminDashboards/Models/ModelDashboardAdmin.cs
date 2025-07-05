namespace SportManager.Application.AdminDashboards.Models;

public class ModelDashboardAdminResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalNewOrder { get; set; }
    public int TotalNewCustomer { get; set; }
    public int TotalVoucher { get; set; }
    public decimal TotalGMV { get; set; }
    public List<LineChartData> LineChartData { get; set; }
}
public class LineChartData
{
    public string Name { get; set; }
    public int? Orders { get; set; } = 0;
}