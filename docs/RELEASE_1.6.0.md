# MAUverce 1.6.0 release record

Built: 2026-07-16 05:04:33 +03:00

Classification: historical engineering release candidate, not a production-signed release.

## Android artifact

- Package: `com.pmi4freal.mauverse3`
- Version: `1.6.0` (`versionCode` 6)
- Target: `net9.0-android`, Android target SDK 35
- File: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 34,421,232 bytes
- SHA-256: `24A7988133FF87257934902457E4EFB042F7148B62442D0819AD96A3A1FBB501`
- Signing status: Android Debug certificate; not approved for production distribution.

The path above is mutable and now points to a newer build. The recorded checksum
is historical because a versioned immutable copy was not archived.

## Settings release scope

- Rebuilt settings menu with persisted system, light, and dark theme segments.
- Added live cache size/file statistics and cache cleanup state.
- Added Android app settings and privacy-safe diagnostics actions.
- Added application version, build, platform, university, and UIT credits.
- Replaced remaining static text/surface colors with semantic theme resources.
- Removed hard-coded light colors from the Android Shell renderer.

## Verification

- Android Release: 0 warnings, 0 errors.
- Upgrade installation: successful with account and preferences preserved.
- Device: RMX2156L1, Android 16 (API 36), 1080 x 2400.
- Dark/light live switching, full scroll, cache statistics, diagnostics copy,
  and Android settings navigation verified on device.
- Final logcat: no fatal exception, ANR, XAML parse, binding, or unhandled exception signatures.
- Warm tab switching: p95 15-20 ms, p99 61-73 ms, no frozen frames.
