using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportManager.Application.Abstractions;
using SportManager.Application.Auths.Models;
using SportManager.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace SportManager.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext dbContext, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(bool Success, string? UserId, string? Username, string? CustomerId, int totalCartItems, IEnumerable<string>? Roles)> ValidateUserAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return (false, null, null, null, 0, null);

        var isPasswordValid = VerifyPasswordHash(request.Password, user.PasswordHash, user.Username);

        if (!isPasswordValid)
            return (false, null, null, null, 0, null);

        var roles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_dbContext.Roles,
                  ur => ur.RoleId,
                  r => r.Id,
                  (ur, r) => r.Name)
            .ToListAsync();

        var cart = await _dbContext.Carts
            .Where(c => c.UserId == user.Id)
            .Include(c => c.Items)
            .FirstOrDefaultAsync();

        string? customerId = null;
        if (user.CustomerProfile != null)
        {
            // Validate thêm nếu cần
            if (user.CustomerProfile.Id != Guid.Empty)
            {
                customerId = user.CustomerProfile.Id.ToString();
            }
            else
            {
                _logger.LogWarning($"CustomerProfile exists but has invalid Id for user {user.Id}");
            }
        }

        int totalCartItems = cart?.Items?.Count ?? 0;

        return (true, user.Id.ToString(), user.Username, customerId, totalCartItems, roles);
    }

    public string HashPassword(string password, string username)
    {
        var saltBytes = Encoding.UTF8.GetBytes(username.ToLower());
        using var hmac = new HMACSHA512(saltBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPasswordHash(string password, string storedHash, string username)
    {
        var saltBytes = Encoding.UTF8.GetBytes(username.ToLower());
        using var hmac = new HMACSHA512(saltBytes);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computedHashBase64 = Convert.ToBase64String(computedHash);
        return computedHashBase64 == storedHash;
    }
}
