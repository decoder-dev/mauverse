using System.Text.Json.Serialization;
using mau.Utils;

namespace mau.DTOModels;

public sealed class UserDTO : BaseDTO
{
    [JsonPropertyName("userid")]
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("fullname")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("roleid")]
    public UserRole Role { get; set; }

    public string CreditBook { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public string SubGroupId { get; set; } = string.Empty;

    [JsonPropertyName("groupname")]
    public string GroupName { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string PrivateToken { get; set; } = string.Empty;
}
