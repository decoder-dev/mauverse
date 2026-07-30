namespace mau.Utils;

public static class UserGreeting
{
    public static string ResolveFirstName(string? firstName, string? fullName, string? username)
    {
        var login = username?.Trim() ?? string.Empty;
        var directName = firstName?.Trim() ?? string.Empty;
        if (directName.Length > 0 &&
            !string.Equals(directName, login, StringComparison.OrdinalIgnoreCase))
        {
            return directName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? "Студент";
        }

        var normalizedFullName = fullName?.Trim() ?? string.Empty;
        if (normalizedFullName.Length == 0 ||
            string.Equals(normalizedFullName, login, StringComparison.OrdinalIgnoreCase))
        {
            return "Студент";
        }

        var parts = normalizedFullName.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 3 ? parts[1] : parts.FirstOrDefault() ?? "Студент";
    }
}
