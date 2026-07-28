# MAUverce 1.6.1 release record

Built: 2026-07-16 11:28:13 +03:00

Classification: engineering release candidate, not production-approved.

## Android artifact

- Application name: `MAUverce`
- Package: `com.pmi4freal.mauverse3`
- Version: `1.6.1` (`versionCode` 7)
- File: `mauverse_mobile/bin/Release/net9.0-android/com.pmi4freal.mauverse3-Signed.apk`
- Size: 34,421,232 bytes
- SHA-256: `FFA382BA83A52CD73F89607DFAE7812F094251BFE1D003D3F9C360EA7ECDBD86`
- Signer: `CN=Android Debug, O=Android, C=US`
- Signing status: debug-signed; a protected production keystore was not used.

## Verification

- Android Release: 0 warnings, 0 errors.
- Backend: 11 tests passing; Ruff passing.
- Previous package id is preserved for an in-place upgrade.
- Device installation is pending because the USB device disconnected from ADB.
- Production promotion is blocked until CI, production signing, and the release checklist pass.

The artifact path is mutable and no longer matches the recorded SHA-256 after a
later failed build attempt. Do not distribute the current file at that path; a
new successful build must be signed, checksummed, smoke-tested, and archived
under a versioned immutable filename.
