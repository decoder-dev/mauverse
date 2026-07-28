namespace mau.DTOModels;

public sealed class RecentMoodleMessages : BaseDTO
{
    public int Id { get; set; }
    public List<UserChatDTO> Members { get; set; } = [];
    public List<MessageDTO> Messages { get; set; } = [];
}
