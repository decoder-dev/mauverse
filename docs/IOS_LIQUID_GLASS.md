# Native iPhone app (SwiftUI)

The shipping iPhone application is the native SwiftUI client in `MauverseIOS`.
Android remains the .NET MAUI client in `mauverse_mobile`. Both talk to the same
MAUverse API and share the same bundle id `com.pmi4freal.mauverse3`.

## Rendering

- iOS 26 and newer use the system Liquid Glass appearance for navigation and
  translucent surfaces.
- iOS 18–25 use native SwiftUI / UIKit materials.
- Reduce Transparency, Increase Contrast, and Reduce Motion remain controlled
  by iOS.

## Build and run

Requires a Mac with Xcode 26.5 (or newer matching `project.yml`) and
[XcodeGen](https://github.com/yonaskolb/XcodeGen):

```bash
brew install xcodegen
cd MauverseIOS
xcodegen generate --spec project.yml
open MAUverse.xcodeproj
```

Set `MAUVERSE_SCHEDULE_TOKEN` in the Xcode build settings or scheme environment
when calling the schedule API. A physical iPhone is recommended for final
material, accessibility, dark-mode, and performance checks.

For App Store distribution, configure the team, provisioning profile, and
release signing on the Mac. Do not publish an unsigned or Debug-signed archive.

## Unsigned GitHub CI artifact

The `ios-release-unsigned` job (display name
`Native SwiftUI iPhone Release (unsigned)`) publishes
`mauverse-native-ios-arm64-release-unsigned`. It contains:

- `mauverse-native-ios-arm64-unsigned.ipa`;
- `checksums.txt`;
- `build-metadata.txt`.

The IPA is only an unsigned container for later signing. The job does not import
an Apple certificate or provisioning profile and fails if the application
contains `_CodeSignature` or `embedded.mobileprovision`. Sign the extracted
application with your own matching certificate, entitlements, and provisioning
profile, then recreate the IPA.

## Portal parity with Android

`MauverseIOS` mirrors the Android portal surfaces from mauniver.ru:

- Guides: Студенту, Абитуриенту, Наука, International
- Digital services hub (ЭИОС, library, webmail, dorm IT, Intra, PROMT)
- Events calendar from `/press/calendar/rss/`
- News filters Абитуриент and Календарь
- Contacts and payment requisites
- Campus navigator with branches, transport tips, route and panorama
- Settings links to official site, personal-data policy, and sveden
- Schedule teacher/room chips (mutually exclusive)

## Required smoke checks

1. Login, logout, and restored session.
2. All five tabs: Home, Schedule, Services, News, and Profile.
3. Service routes, portal guides, calendar, contacts, in-app browser, messenger, and forms.
4. Light and dark appearance.
5. iOS Reduce Transparency and Increase Contrast.
6. Dynamic Type and VoiceOver labels.
7. iPhone portrait and landscape layouts.
