# MAUverce 1.7.2 release record

Built: 2026-07-16 19:56:48 +03:00

Classification: engineering release candidate, not production-approved.

## Android artifact

- Application name: `MAUverce`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.7.2` (`versionCode` 10)
- Archived file: `.artifacts/MAUverce-1.7.2-android-engineering.apk`
- Build output: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 35,191,785 bytes
- SHA-256: `70BD8CA41014E8EC08892F03C2BCEC4DADF3D7DF23677B1463F985BD0D3AAAA9`
- Signer: `CN=Android Debug, O=Android, C=US`
- Certificate SHA-256: `496a8124c217942588e14db3aa593605d01b6ae42ab5866f0ec74adc6c4d8653`

## Authentication and authorization

- Login errors distinguish invalid credentials, rate limits, connectivity, timeout, and upstream outages without exposing technical details.
- Passwords and transient request credentials are cleared after failed or cancelled login attempts.
- Restored sessions are checked through the existing protected API before normal navigation.
- `401/403` responses clear local data, request headers, and Android `SecureStorage`; dependency outages retain offline data.
- Moodle credential errors are separated from internal Moodle failures.
- Authentication attempts are limited independently by client IP and normalized username.
- Student debt access verifies a unique server-side credit-book owner against Moodle identity and the authoritative local group.

## Backend and source archive

- Backend tests: 57/57 passing.
- Ruff, compileall, and `pip check`: passing.
- `mauverse-api.zip` was rebuilt from the current source tree without `.env`, virtual environments, caches, or legacy credentials.
- Sanitized API archive size: 51,065 bytes.
- Sanitized API archive SHA-256: `4D400B9144CDE101A89945C8A5B8582918931FEA989F829553A01CF6339CC67D`.
- `mauverse_mobile.zip` was rebuilt from the 1.7.2 source tree without IDE state, `bin`, `obj`, local databases, or user-specific project files.
- Sanitized mobile archive size: 3,030,213 bytes.
- Sanitized mobile archive SHA-256: `F2CF46D065F55C3C7E728AFBA7AAE30F8B386771CF54EC1FA83F7D7544E997B9`.

## Verification

- Android Debug and Release builds with `--warnaserror`: 0 warnings, 0 errors.
- `dotnet format --verify-no-changes`: passing.
- APK manifest reports package `com.pmi4freal.mauverse3`, version `1.7.2`, version code `10`, minimum SDK 21, and target SDK 35.
- APK signature verifies with v1, v2, and v3 schemes.
- The previous package id and signing identity are preserved for an in-place engineering upgrade.

## Promotion gates

- Device installation and real-account smoke testing remain pending because no authorized USB device is visible to ADB.
- The database credentials exposed by the removed legacy archive must be rotated and their access logs reviewed by the infrastructure owner.
- Production promotion still requires the protected release keystore, hosted CI on the exact source revision, supported-device testing, and deployment of the matching API release.
