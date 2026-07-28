using System.ComponentModel.DataAnnotations.Schema;
using mau.Utils;

namespace mau.Models;

public sealed class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string CreditBook { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string SubGroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string GroupDescription { get; set; } = string.Empty;

    [NotMapped]
    public string Token { get; set; } = string.Empty;

    [NotMapped]
    public string PrivateToken { get; set; } = string.Empty;
}
