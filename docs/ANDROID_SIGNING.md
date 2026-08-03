# Android signing (engineering)

Production Play keystores stay out of the repository (see `docs/CI_AND_RELEASE.md`).
For local sideload / engineering Release builds:

1. Generate a keystore (already done under `.artifacts/signing/` in cloud agents; regenerate locally if needed):

```bash
keytool -genkeypair -v \
  -keystore .artifacts/signing/mauverse-engineering.keystore \
  -alias mauverse \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -storepass mauverse-eng \
  -keypass mauverse-eng \
  -dname "CN=MAUverse Engineering, OU=MAUverse, O=decoder-dev, L=Murmansk, ST=Murmansk, C=RU"
```

2. Copy `mauverse_mobile/Directory.Build.signing.props.example` → `mauverse_mobile/Directory.Build.signing.props` (gitignored).

3. Build:

```bash
cd mauverse_mobile
dotnet build mau.csproj -c Release -f net10.0-android -p:AndroidPackageFormats=aab
```

4. Verify signer is not missing:

```bash
jarsigner -verify -verbose -certs \
  bin/Release/net10.0-android/android-arm64/com.pmi4freal.mauverse3-Signed.aab | head
```

Engineering cert `CN=MAUverse Engineering` is for sideload only — not for Play Store.
