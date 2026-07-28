namespace mau.Models;

public sealed class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

#pragma warning disable CA1707 // Existing EF queries and persisted schema use this legacy member name.
    public int Schedule_id { get; set; }
#pragma warning restore CA1707
}
