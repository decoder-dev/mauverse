# MAUverce

MAUverce is a student mobile application for Murmansk Arctic University. The
repository contains a .NET 9 MAUI client and a FastAPI gateway.

## Repository layout

- `mauverse_mobile` - .NET MAUI application for Android and iPhone.
- `mauverse-api` - FastAPI gateway for EIOS, schedules, news, debts, contacts,
  and student forms.
- `.github/workflows/ci.yml` - backend, container, and Android verification.
- `docs` - production audit, release process, and release records.

## Backend development

Python 3.11 is the supported runtime.

```powershell
cd mauverse-api
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install --requirement requirements-dev.txt
Copy-Item .env.example .env
python main.py server main --reload
```

Populate every required database value in `.env`; never commit that file.

Run the same checks used by CI:

```powershell
python -m pip check
python -m unittest discover -s tests -v
ruff check .
python -m compileall -q apps tests main.py
```

## API container

The image is pinned to a multi-architecture Python base digest, runs as UID/GID
`10001`, and exposes a liveness healthcheck at `/health`.

```powershell
docker build --pull --tag mauverce-api:local mauverse-api
docker run --rm --env-file mauverse-api/.env -p 8000:8000 mauverce-api:local
```

`/health` confirms that the process can serve HTTP. `/ready` checks both database
connections and returns `503` when either is unavailable. Moodle and mail remain
runtime dependencies and require separate monitoring and incident policy.

## Android development

Install .NET SDK `9.0.313`, JDK 17, Android SDK, and the MAUI Android workload.

```powershell
cd mauverse_mobile
dotnet workload install maui-android --skip-manifest-update
dotnet restore mau.csproj
dotnet build mau.csproj --configuration Release --framework net9.0-android --no-restore
```

The API endpoint can be overridden without changing source:

```powershell
dotnet build mau.csproj --configuration Release --framework net9.0-android `
  -p:MauverseApiBaseUrl=https://example.edu/campus-api/
```

The Android toolchain may generate a `*-Signed.apk` using the Android Debug
certificate when no production keystore is configured. That file is an
engineering build and must not be distributed as a production release.

## iPhone development

The iPhone target keeps the full MAUverse feature set and uses native Liquid
Glass navigation and content surfaces on iOS 26, with an adaptive UIKit material
fallback on iOS 15-25. Building and signing iOS requires a Mac with Xcode 26 or
newer. See [iPhone and Liquid Glass](docs/IOS_LIQUID_GLASS.md).

## CI and releases

GitHub Actions uses minimal `contents: read` permission, immutable action SHAs,
pip and NuGet caches, bounded job timeouts, and concurrency cancellation. It
runs backend tests, Ruff, compileall, a non-root container health smoke test,
Android and iPhone Release-configuration builds. CI uploads unsigned Android
packages and an unsigned `ios-arm64` IPA container; it never imports signing
certificates or provisioning profiles.

Production signing, device smoke testing, deployment, and rollback are explicit
promotion gates. See [CI and release engineering](docs/CI_AND_RELEASE.md), the
[production audit](docs/PRODUCTION_AUDIT.md), and the
[release checklist](docs/RELEASE_CHECKLIST.md). Naming, comments, and review
rules are defined in [code style](docs/CODE_STYLE.md).
