# MAUverce 1.7.3 release record

Built: 2026-07-16 20:29:16 +03:00

Classification: engineering release candidate, not production-approved.

## Android artifact

- Application name: `MAUverce`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.7.3` (`versionCode` 11)
- Archived file: `.artifacts/MAUverce-1.7.3-android-engineering.apk`
- Size: 35,356,051 bytes
- SHA-256: `D39502EA06E440586610F8C3F83345DEB428801F2EC6CC9DDA7AA4DCFCE68083`
- Signer: `CN=Android Debug, O=Android, C=US`
- Certificate SHA-256: `496a8124c217942588e14db3aa593605d01b6ae42ab5866f0ec74adc6c4d8653`

## Startup experience

- Android splash assets now use an explicit 128 dp base size instead of generating 1000-4000 px launch images.
- The native splash and MAUI initialization page use the same untinted MAUverce mark.
- Dark-mode startup uses the application surface color rather than a blue fallback.
- SQLite creation moved out of `DbContext` construction and runs asynchronously after the initialization page is visible.
- Local schedule loading and online session validation run concurrently during a normal startup.
- Schedule refresh no longer performs a redundant session request before its already protected API call.

## Verification

- Android Debug and Release builds with `--warnaserror`: 0 warnings, 0 errors.
- `dotnet format --verify-no-changes`: passing.
- XAML XML validation: 39/39 files passing.
- Generated splash images: 128, 192, 256, 384, and 512 px for Android density buckets.
- APK manifest reports version `1.7.3`, version code `11`, minimum SDK 21, and target SDK 35.
- The backend remains the hardened 1.7.2 API with 57/57 tests passing.

## Source archive

- `mauverse_mobile.zip` size: 3,033,808 bytes.
- `mauverse_mobile.zip` SHA-256: `4620FACB59E5818DE45B72665B267010BCF7939A032F832D6AC5D45E61BB2727`.
- IDE state, build output, local databases, keystores, and user-specific project files are excluded.

## Promotion gates

- Device installation and startup timing remain pending because no authorized USB device is visible to ADB.
- Production promotion requires the protected release keystore and a cold-start smoke test on supported Android versions.
