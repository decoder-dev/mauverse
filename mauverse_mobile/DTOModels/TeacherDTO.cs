namespace mau.DTOModels;

public sealed class TeacherDTO : BaseDTO
{
    public IEnumerable<string> Teachers { get; set; } = [];
}
