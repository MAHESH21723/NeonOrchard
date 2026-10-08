# Android package verification

Verified 7 October 2026 for `NeonOrchard-test.apk`.

- Unity build: **Succeeded**, zero errors.
- APK size: approximately 25 MB.
- Application ID: `com.neonorchard.snake`.
- Version: 1.0.0 / code 1.
- Minimum Android API 26; target and compile API 36.
- Native ABI: ARM64 (`arm64-v8a`).
- APK signature verified by Android `apksigner` (v2 signature). Uses a debug/test key; not a store upload artifact.
- APK ZIP alignment verified by `zipalign -c -P 16 4`.
- All seven native libraries inspected with NDK `llvm-readelf`; every LOAD segment alignment is `0x4000` (16 KB).
- No INTERNET or ACCESS_NETWORK_STATE permission in the final APK. Permissions are VIBRATE and the AndroidX app-specific receiver permission.
- Analytics, engine diagnostics and crash reporting disabled in project configuration.
- Font attribution included as a Resources text asset and as `FontLicense.txt` in this release folder.
- One build warning states that Pipeline automation is disabled in Player builds; this is intended.

SHA-256:

`41E2FCCE03574DB451CE552BEEFC10983EAE535EE6254857BADF7B75CDAECD50`

These are build, signature, manifest and binary-alignment checks. The app has not been run on an Android phone or a 16 KB Android emulator in this session because no device was connected. Physical-device testing and a Play pre-launch report remain before publication.

The signed store AAB, publisher contact details, hosted privacy policy and Play Console forms are pending. See `ReleaseChecklist.md`.
