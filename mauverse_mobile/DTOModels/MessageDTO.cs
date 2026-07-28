namespace mau.DTOModels;

public sealed class MessageDTO : BaseDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FullMessage { get; set; } = string.Empty;
    public int UserIdFrom { get; set; }
    public string FullnameFrom { get; set; } = string.Empty;
    public long TimeCreated { get; set; }
    public string TimeCreateString { get; set; } = string.Empty;
}
