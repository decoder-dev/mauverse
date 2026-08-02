namespace mau.Models;

public sealed record UniversityGuideSection(
    string Title,
    IReadOnlyList<UniversityGuideLink> Links);
