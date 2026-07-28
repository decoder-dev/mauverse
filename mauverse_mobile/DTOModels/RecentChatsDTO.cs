namespace mau.DTOModels;

public sealed class RecentChatsDTO : BaseDTO
{
    public int Id { get; set; }
    public UserChatDTO User { get; set; } = new();
    public MessageDTO Message { get; set; } = new();
}
