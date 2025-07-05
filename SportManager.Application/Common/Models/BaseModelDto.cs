namespace SportManager.Application.Common.Models;

public class BaseModelDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
