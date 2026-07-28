# iPhone and Liquid Glass

The iPhone application uses the same MAUverse pages, view models, API clients,
cache, authentication, and navigation routes as the Android application.

## Rendering

- iOS 26 and newer use `UIGlassEffect` for marked content surfaces.
- iOS 15 through 25 fall back to the adaptive UIKit system material.
- Shell navigation and tab bars keep their native translucent appearance, so
  iOS 26 renders Apple's current Liquid Glass treatment automatically.
- Reduce Transparency, Increase Contrast, and Reduce Motion remain controlled
  by iOS because the implementation uses native UIKit materials.

## Build and run

Use a Mac with .NET 10, the .NET MAUI workload, Xcode 26 or newer, and an Apple
development signing identity:

```bash
cd mauverse_mobile
dotnet workload restore
dotnet restore mau.csproj
dotnet build mau.csproj -f net10.0-ios -c Debug
```

Open the solution in Visual Studio Code with the .NET MAUI extension or use the
command line to select an iPhone simulator/device. A physical iPhone is
recommended for the final material, accessibility, dark-mode, and performance
checks.

For App Store distribution, configure the team, provisioning profile, and
release signing on the Mac. Do not publish a Debug-signed archive.

## Unsigned GitHub CI artifact

The `ios-release-unsigned` GitHub Actions job publishes
`mauverce-ios-arm64-release-unsigned`. It contains:

- `mauverce-ios-arm64-unsigned.ipa`;
- `checksums.txt`;
- `build-metadata.txt`.

The IPA is only an unsigned container for later signing. The job does not import
an Apple certificate or provisioning profile and verifies that the application
contains neither `_CodeSignature` nor `embedded.mobileprovision` before upload.
Sign the extracted application with your own matching certificate, entitlements,
and provisioning profile, then recreate the IPA. Never store an unprotected
private key in the repository or in a downloadable CI artifact.

## Required smoke checks

1. Login, logout, and restored session.
2. All five tabs: Home, Schedule, Services, News, and Profile.
3. Service routes, popups, embedded browser, messenger, and forms.
4. Light and dark appearance.
5. iOS Reduce Transparency and Increase Contrast.
6. Dynamic Type and VoiceOver labels.
7. iPhone portrait and landscape layouts.
