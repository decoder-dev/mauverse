# MAUverce 1.7.1 release record

Built: 2026-07-16 16:49:48 +03:00

Classification: engineering release candidate, not production-approved.

## Android artifact

- Application name: `MAUverce`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.7.1` (`versionCode` 9)
- Archived file: `.artifacts/MAUverce-1.7.1-android-engineering.apk`
- Build output: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 35,191,785 bytes
- SHA-256: `5E72C9B7C2D02394FEA33166C90E6DE3B3F994CB9BAB723C692183D0F6A896D1`
- Signer: `CN=Android Debug, O=Android, C=US`
- Certificate SHA-256: `496a8124c217942588e14db3aa593605d01b6ae42ab5866f0ec74adc6c4d8653`

## Quality pass

- C# follows enforced .NET naming conventions with contract-safe JSON, EF, Shell, and XAML mappings.
- Python follows strict Ruff formatting, typing, security, performance, and naming rules.
- Comments remain only around non-obvious migration, concurrency, cache, security, and Android behavior.
- Mobile request cancellation, cache writes, SQLite initialization, error handling, and DI lifetimes were hardened.
- UI sizing, dark-theme contrast, TalkBack metadata, 48 dp touch targets, insets, and list virtualization were audited.
- Backend redirects, cookies, secrets, authorization boundaries, SQL parameters, and mobile HTTP contracts were hardened.

## Verification

- Android Release with `--warnaserror`: 0 warnings, 0 errors.
- `dotnet format --verify-no-changes`: passing, including naming analyzers.
- XAML XML validation: 39/39 files passing.
- Backend: 33/33 tests passing; Ruff format/lint, compileall, and `pip check` passing.
- APK manifest and certificate verified with Android build tools.
- Source scan found no TODO/FIXME, merge markers, fake loading rows, skeleton paths, or `Console.WriteLine`.
- Previous package id is preserved for an in-place upgrade.

## Promotion gates

- Device installation is pending because no authorized USB device is visible to ADB.
- Hosted CI must pass on the exact release commit.
- Production promotion requires the protected release keystore, device/TalkBack tests, privacy and crash-reporting approval, and Apple-platform verification.
