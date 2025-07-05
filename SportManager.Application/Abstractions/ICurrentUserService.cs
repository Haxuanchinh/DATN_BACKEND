namespace SportManager.Application.Abstractions;

public interface ICurrentUserService
{
    string UserId { get; }
    string Username { get; }
    string? CustomerId { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}
