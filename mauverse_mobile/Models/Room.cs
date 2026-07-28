namespace mau.Models;

public sealed class Room
{
    public int RoomId { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
