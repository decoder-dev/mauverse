namespace mau.DTOModels;

public sealed class UserChatDTO : BaseDTO
{
    public int Id { get; set; }
    public int ConvId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
