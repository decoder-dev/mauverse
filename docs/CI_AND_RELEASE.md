# MAUverce CI and release engineering

## Continuous integration

`.github/workflows/ci.yml` runs on pushes, pull requests, and manual dispatch.
The workflow requests only `contents: read` and cancels superseded runs on the
same ref.

| Job | Verification | Output |
| --- | --- | --- |
| `backend` | Python 3.11 install, all discovered unit tests, `pip check`, Ruff, compileall | No artifact |
| `api-image` | Docker build, UID is non-root, container reaches healthy state | No registry push |
| `android-release` | SDK from `global.json`, JDK 17, MAUI workload, mobile tests, warning-free Release build | Unsigned APK, AAB, and SHA-256 file, retained 14 days |
| `ios-release-unsigned` | macOS 26, Xcode 26.5, XcodeGen, unsigned SwiftUI `MauverseIOS` device build, signature/provisioning audit | Unsigned native IPA container, build metadata, and SHA-256 file, retained 14 days |

All GitHub-maintained actions are referenced by immutable commit SHA with the
major version recorded in a comment. Dependabot checks those references and the
Python, NuGet, and Docker dependencies weekly.

## Reproduction

Backend commands are run from `mauverse-api`:

```text
python -m pip install --requirement requirements-dev.txt
python -m pip check
python -m unittest discover -s tests -v
ruff check .
python -m compileall -q apps tests main.py
```

The Android build is run from `mauverse_mobile`:

```text
dotnet workload install maui-android --skip-manifest-update
dotnet restore mau.sln
dotnet test ../mauverse_mobile.Tests/mauverse_mobile.Tests.csproj \
  --configuration Release --no-restore --warnaserror
dotnet build mau.csproj --configuration Release --framework net10.0-android \
  --no-restore --warnaserror -p:ContinuousIntegrationBuild=true -maxcpucount:1
```

The CI artifact is intentionally unsigned. A green Android job proves that the
Release configuration compiles; it does not authorize distribution.

The iPhone job builds the native SwiftUI app in `MauverseIOS` and produces
`mauverse-native-ios-arm64-unsigned.ipa`. This is a ZIP container with an
unsigned device `.app` under `Payload/`; it is not installable until it is
signed with an Apple Distribution or Apple Development certificate and a
matching provisioning profile. CI imports no certificate and fails if
`_CodeSignature` or `embedded.mobileprovision` is present. The schedule API
token is injected at build time from the `MAUVERSE_SCHEDULE_TOKEN` repository
secret.

## Production promotion

1. Require all four CI jobs on the exact release commit.
2. Confirm the production API URL, TLS ownership, privacy text, mail recipients,
   rate limiting, monitoring, and rollback owner.
3. Build in an approved signing environment using a protected production
   keystore. Never pass keystore passwords as command-line text or print them.
4. Verify that the APK signer is the approved production certificate and is not
   `CN=Android Debug`.
5. Record the immutable commit, application version/code, artifact name, size,
   SHA-256, certificate SHA-256, builder, and build timestamp.
6. Install the exact checksummed artifact as an in-place upgrade and complete
   `docs/RELEASE_CHECKLIST.md` on supported devices.
7. Archive the artifact under a versioned, immutable name and retain the prior
   approved package for rollback.

Production keystores and passwords belong in the approved secret manager or
signing service. They are deliberately absent from this repository and CI.

## Container promotion

The Dockerfile is pinned to a Python multi-architecture digest and runs as
UID/GID `10001`. `/health` is the container liveness check; the orchestrator
should use `/ready` for database readiness. Deployment must still configure TLS
termination, Moodle/mail monitoring, redacted centralized logs, resource limits,
alerts, and edge rate limiting before traffic promotion.
