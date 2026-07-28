using mau.DTOModels;

namespace mau.Models;

public sealed class UniversityInfo : BaseDTO
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string Extras { get; set; } = string.Empty;
}
