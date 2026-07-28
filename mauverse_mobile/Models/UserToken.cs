namespace mau.Models;

public sealed class UserToken
{
    public string PrivateToken { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? Error { get; set; }
}
