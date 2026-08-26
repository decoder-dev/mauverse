# UX / bug audit — dumbest-user schemes (1.12.3)

Built: 2026-08-03  
Clients: Android MAUI (`mauverse_mobile`) + iOS SwiftUI (`MauverseIOS`)

Perspective: a confused freshman who skips text, taps the biggest control, and assumes every blue thing calls or opens something.

---

## Convenient (keep)

- Five clear tabs; Russian snackbars; offline schedule/news caches.
- Login field validation; logout wipe confirmation.
- Campus route/panorama; contacts call/copy; Murmansk phone + extension dialing.
- iOS Keychain session; news HTML entity cleaning on RSS path.
- In-app browser chrome (back/forward/reload).

---

## Scheme A — Bugs (fix)

| ID | Issue | Platform | Fix |
| --- | --- | --- | --- |
| B1 | `CanStateChange` defaults false → Loading/Empty never shows | Android | Init `CanStateChange = true` in `BaseViewModel` |
| B2 | News Appearing always reloads Default; chip desync | Android | `LoadData` uses `SelectedButton?.FilterType` |
| B3 | News/events network fail shown as “empty category” | Android | Distinct error empty copy / snackbar + state |
| B4 | Login subgroup dismiss saves empty subgroup | Android | Require subgroup or send to Profile setup |
| B5 | `CurrentUser` null races after logout | Android | Null-guards on Profile/Schedule |
| B6 | News HTML entities in detail popup | Android | HtmlDecode/strip before bind |
| B7 | Moodle `groupId` treated as schedule UID | iOS | Stop copying; always resolve via `findGroup` |
| B8 | Native certificate form never linked | iOS | Wire `CertificateRequestView` like Android `IsNative` |
| B9 | Hardcoded `/dev/mauverse/` API base | iOS | Build-configurable base URL (parity with Android) |
| B10 | Teacher search needs «Найти» after suggestion tap | Android | Auto-run search on teacher select |
| B11 | Android CA1822 / Release already fixed | — | done in 1.12.2 |

---

## Scheme B — Inconvenient (improve)

| ID | Pain | Platform | Fix |
| --- | --- | --- | --- |
| U1 | Tab «Пары» vs page «Расписание» | Android | Rename tab to «Расписание» |
| U2 | Legend «Нет занятий» always looks like today empty | Android | Reword caption |
| U3 | Teacher short query shown as wifi «Ошибка» | iOS | Hint style, not error icon |
| U4 | Home pushes duplicate tab roots | iOS | Switch selected tab instead of NavigationLink push |
| U5 | Schedule group row not tappable | iOS | Link to Profile editor |
| U6 | Soften Keychain / ЭИОС jargon | Both | Friendlier labels |
| U7 | Certificate DOB prefilled 01.01.2000 | Android | Empty until chosen |
| U8 | International guide English subtitle | Android | Russian description |
| U9 | Date chips English locale | iOS | `ru_RU` |
| U10 | Home error without Retry | iOS | Retry button |
| U11 | Phone label not obvious call CTA | Android | Keep Primary color + ensure tap works |
| U12 | Unparseable phone hidden | iOS | Show number + «Не удалось набрать» path |

---

## Scheme C — Android signing (end)

| Step | Action |
| --- | --- |
| C1 | Generate local engineering upload keystore (gitignored) |
| C2 | Add optional signing props via `Directory.Build.signing.props` (gitignored) + documented template |
| C3 | Document `keytool` + `dotnet build` with signing props |
| C4 | Produce signed AAB/APK and verify signer ≠ missing; engineering cert OK for sideload |

Production Play keystore stays out of repo (existing policy).

---

## Implementation status (2026-08-03)

Implemented on branch `decoder-dev/ux-audit-fixes-010d` for release **1.12.3**:

- Android: B1–B6, B10, U1–U2, U6–U8 (+ profile/schedule null guards)
- iOS: B7–B9, U3–U6, U9–U10, U12, B8 (+ mailto on contacts)
- Signing: C1–C4 docs + engineering keystore path + optional `Directory.Build.signing.props`

Deferred (follow-up): soft refresh without LoadingPage, telephone search, teacher schedule role parity, settings cache confirm.
