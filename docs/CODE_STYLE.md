# MAUverce code style

## C# and XAML

- Types, public methods, properties, commands, and events use `PascalCase`.
- Parameters and local values use `camelCase`; private fields use `_camelCase`.
- Interfaces use the `IName` convention.
- Existing JSON, SQLite, Shell query, and XAML binding names are compatibility
  contracts. Rename them only together with an explicit mapping or migration.
- Async I/O accepts and propagates a `CancellationToken` where the caller owns a
  lifecycle.
- Comments explain intent, constraints, security decisions, or non-obvious
  algorithms. They must not repeat what the next line already says.

## Python

- Modules, functions, parameters, and variables use `snake_case`; classes use
  `PascalCase`; constants use `UPPER_SNAKE_CASE`.
- Public HTTP paths and JSON fields are compatibility contracts and are not
  renamed as part of internal cleanup.
- Docstrings and comments document security boundaries, concurrency behavior,
  retry policy, and unusual upstream contracts.

## Verification

- C#: `dotnet build mau.csproj -f net10.0-android -c Release --warnaserror`
- Mobile tests: `dotnet test ../mauverse_mobile.Tests/mauverse_mobile.Tests.csproj -c Release --warnaserror`
- Formatting: `dotnet format mau.sln --verify-no-changes --no-restore`
- Python: `ruff check .` and `python -m unittest discover -s tests -v`
- XAML: Release XamlC build plus device checks for light/dark theme, large text,
  TalkBack, system insets, tab switching, and scrolling.
