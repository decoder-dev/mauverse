# MAUverce 1.5.0 release record

Built: 2026-07-16 04:14:47 +03:00

Classification: historical engineering release candidate, not a production-signed release.

## Android artifact

- Package: `com.pmi4freal.mauverse3`
- Version: `1.5.0` (`versionCode` 5)
- Target: `net9.0-android`, Android target SDK 35
- File: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 34,417,136 bytes
- SHA-256: `077C1CBBAAC8596CF38AA44C7EBEB850D9877E12799EA29062DB73FEAFDE8A60`
- Signing status: Android Debug certificate; not approved for production distribution.

The path above is mutable and now points to a newer build. The recorded checksum
is historical because a versioned immutable copy was not archived.

## Verification

- Android Release: 0 warnings, 0 errors.
- Backend: 11 tests passing; Ruff and compileall passing.
- Upgrade installation: successful with existing user data preserved.
- Device smoke: RMX2156L1, Android 16 (API 36), 1080 x 2400.
- Final logcat: no fatal exception, ANR, XAML parse, binding, or unhandled exception signatures.
- Warm tab switching: p95 15-28 ms, p99 65-81 ms, no frozen frames.
