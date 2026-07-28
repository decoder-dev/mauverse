using mau.Models;
using mau.Utils;

namespace mau.DTOModels;

public sealed class UserInfoDTO : BaseDTO
{
    public string GroupName { get; set; } = string.Empty;
    public UserRole RoleId { get; set; } = UserRole.All;
    public string Username { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public IEnumerable<SubGroup> SubGroups { get; set; } = [];
}
