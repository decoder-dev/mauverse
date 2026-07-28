namespace mau.Models;

public sealed record StudentOnlineForm(
    string Title,
    string Description,
    string Url,
    string Glyph,
    bool IsNative = false);
