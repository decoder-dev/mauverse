using mau.Models;

namespace mau.DTOModels;

public sealed class SubGroupDTO : BaseDTO
{
    public string GroupId { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public IEnumerable<SubGroup> SubGroups { get; set; } = [];
}
