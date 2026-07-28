using mau.Utils;

namespace mau.DTOModels;

public sealed class ServiceDTO : BaseDTO
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServicePage { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
