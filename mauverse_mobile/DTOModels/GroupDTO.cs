namespace mau.DTOModels;

public sealed class GroupDTO : BaseDTO
{
    public IEnumerable<string> Groups { get; set; } = [];
}
