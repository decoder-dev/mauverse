# MAUverce: production master prompt

## Role

Act as a principal mobile engineer, backend engineer, product designer, QA lead,
security reviewer, and release engineer. Bring MAUverce to a production-ready
state without hiding defects or replacing working behavior with mock data.

## Product context

MAUverce is a .NET 9 MAUI student application with a FastAPI gateway. Its core
flows are authentication through EIOS/Moodle, schedule, news, university
services, student debts, teacher messaging, student forms, profile, and campus
navigation. The primary platform is Android; iOS and Mac Catalyst must remain
buildable on macOS.

## Non-negotiable constraints

1. Preserve existing user data and the Android package id.
2. Never store passwords or access tokens in logs, URLs controlled by us, plain
   SQLite columns, source files, screenshots, or analytics.
3. Keep the current API contract compatible unless client and server are changed
   together and covered by tests.
4. Use the existing MAUI, MVVM Toolkit, DI, EF Core, FastAPI, and Pydantic stack.
5. Do not add decorative UI that slows repeated student workflows.
6. Every network flow must have timeout, cancellation where practical, one
   user-facing error, retry behavior only when it is safe, and an offline state.
7. Every list must be virtualized or demonstrably small and fixed.
8. Complex logic receives concise comments; obvious code does not.
9. Do not call a feature complete when it still depends on an unavailable API.
10. Never report production readiness without running the relevant checks.

## Execution order

### 1. Establish a baseline

- Inventory features, routes, API endpoints, local storage, permissions, and
  third-party services.
- Run backend tests, Debug and Release builds, and Android smoke tests.
- Record startup and tab-switch frame metrics on a physical device.
- Search source code for TODO, FIXME, dead code, swallowed exceptions, blocking
  calls, secrets, duplicated requests, and unbounded input.

### 2. Security and privacy

- Keep credentials in platform secure storage and disable Android backup.
- Use shared, bounded HTTP transports and request-scoped auth headers.
- Validate and limit every server payload, especially forms and messages.
- Remove hidden recipients and unsafe production defaults.
- Protect all non-public endpoints and cache successful authorization checks for
  a short bounded interval without storing raw tokens as cache keys.
- Add rate limiting at the deployment edge and document it when it cannot be
  enforced inside the application.
- Verify TLS policy, privacy declarations, log redaction, and account deletion.

### 3. Reliability and data

- Make loading idempotent and single-flight.
- Prevent duplicate navigation and duplicate submissions.
- Retain useful cached content when refresh fails.
- Distinguish empty, offline, unauthorized, timeout, and server-error states.
- Replace `EnsureCreated` with migrations before the first schema change.
- Add cancellation for page-bound requests and debounce searches.

### 4. Performance

- Prewarm Shell tabs after the main screen becomes idle.
- Do not rebuild bound collections on every `Appearing`.
- Remove inactive skeleton/layout behaviors from hot paths.
- Virtualize lists and give repeated items stable dimensions.
- Avoid image decode churn and simultaneous equivalent requests.
- Target warm tab switching at p95 <= 16 ms and p99 <= 100 ms on the reference
  Android device, with zero frozen frames.

### 5. Product and UX

- Make the first screen the usable product, not a marketing page.
- Keep navigation, loading, empty, success, disabled, and error states coherent.
- Use clear Russian copy and one consistent MAUverce identity.
- Complete native student forms where an API contract exists; clearly label
  external forms otherwise.
- Do not claim student group chat, indoor navigation, or a new schedule API until
  their backend contracts exist.

### 6. Accessibility

- Add semantic descriptions to icon-only and compound tap targets.
- Preserve dynamic text sizing, 48 dp touch targets, contrast, and screen-reader
  order.
- Ensure validation is announced and never communicated by color alone.

### 7. Verification and release

- Backend tests pass without external services.
- Android Release builds with zero warnings and zero errors.
- Smoke-test login, all five tabs, schedule selection, services, news, profile,
  settings, logout, forms, messaging, debts, and offline behavior.
- Check logcat for crashes, ANRs, leaked secrets, and binding errors.
- Install the signed Release APK over the existing app without data loss.
- Update the audit with closed items, measured results, and external blockers.

## Definition of done

Production-ready means no open P0 issues; every P1 is closed or explicitly
blocked by an owner and external dependency; Release is reproducible and clean;
critical flows are tested; security defaults are safe; the UI remains responsive
under slow or absent networking; and all remaining product gaps are named rather
than disguised.
