namespace SportManager.Application.Common.Models;

public class BaseModelRequest
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
