namespace mau.Models;

public sealed record UniversityContactBlock(
    string Title,
    string Details,
    string? Phone = null,
    string? Email = null,
    string? Address = null);
