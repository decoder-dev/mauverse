namespace mau.DTOModels;

public sealed class ChatMessagesDTO : BaseDTO
{
    public int Id { get; set; }
    public List<MessageDTO> Messages { get; set; } = [];
}
