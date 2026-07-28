# MAUverce release checklist

Every checkbox applies to the exact commit and artifact being promoted. An
unchecked release gate is not implicitly waived.

## Source and CI

- [ ] Release commit is immutable, reviewed, and identified in the release record.
- [ ] `backend`, `api-image`, and `android-release` CI jobs pass on that commit.
- [ ] Dependency update alerts have no unaccepted critical findings.
- [ ] `.env`, signing keys, tokens, passwords, and personal data are absent from source and artifacts.
- [ ] Production API URL and TLS ownership are approved.
- [ ] API serves its complete TLS chain; the bundled R13 compatibility anchor is rotated or removed before 2027-03-12.
- [ ] Privacy text, form recipients, retention, and incident owners are approved.

## Android artifact

- [ ] Version name and monotonically increasing version code are confirmed.
- [ ] Release build completes with zero warnings and zero errors.
- [ ] Artifact is signed by the approved production certificate, not `CN=Android Debug`.
- [ ] APK signature verification passes after signing.
- [ ] Versioned artifact filename, byte size, SHA-256, certificate SHA-256, commit, and builder are recorded.
- [ ] Archived artifact checksum matches the tested artifact.

## Device smoke test

- [ ] Upgrade installation preserves login, notes, preferences, and cached schedule.
- [ ] Fresh install, login, logout, timeout, and offline paths behave correctly.
- [ ] All five tabs open and return without frozen frames.
- [ ] Schedule selection, refresh, filters, and notes work.
- [ ] News loading, categories, images, and details work.
- [ ] Services, debts, contacts, implemented forms, navigation, and EIOS links work.
- [ ] Profile edit, settings, cache clear, diagnostics, permissions, and themes work.
- [ ] System, light, and dark themes remain legible after restart.
- [ ] TalkBack order and labels are verified on primary flows.
- [ ] Logcat contains no crash, ANR, binding failure, credential, or token output.

## API deployment

- [ ] Container runs as non-root and reaches healthy state without reload mode.
- [ ] `/health` liveness and `/ready` database readiness probes pass in the deployment environment.
- [ ] Moodle/mail monitoring, redacted logs, metrics, alerts, resource limits, and edge rate limits are enabled.
- [ ] Database, Moodle, and mail timeouts and incident ownership are confirmed.
- [ ] Database compatibility and rollback are tested before traffic migration.
- [ ] API smoke tests pass through the production TLS endpoint.

## Promotion and rollback

- [ ] The exact device-tested APK is promoted without rebuilding.
- [ ] Staged rollout owner, stop conditions, and monitoring window are recorded.
- [ ] Previous approved APK/image and API rollback procedure are available.
- [ ] Release record is completed only after all required gates pass.
