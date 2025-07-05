namespace SportManager.Application.Auths.Models;

public class LoginRequest : IRequest<LoginResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public int TotalCartItems { get; set; }
    public IEnumerable<string> Roles { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class LogoutResponse
{
    public bool IsLogoutSucess { get; set; }
}

public class RefreshTokenRequest : IRequest<LoginResponse>
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class LogoutRequest : IRequest<LogoutResponse>
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}