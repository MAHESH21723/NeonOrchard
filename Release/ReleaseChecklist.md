# Release handoff

## Project and deliverables

Open the Unity project `NeonOrchard` in Unity 6000.3.10f1. Open `Assets/NeonOrchard/Scenes/Orchard.unity` and press Play.

The test APK uses Android debug signing and is for installation/testing. It must not be submitted as the Play release. A store-signed AAB requires the publisher's upload keystore. Never upload the signing key or passwords with the store assets or source archive.

## Before the first upload

1. Confirm the final developer/studio name and public support email. The working product name is Neon Orchard and the initial application ID is `com.neonorchard.snake`. Verify that this ID is available in your Play Console before committing it to a release. Update `OrchardBuilder.Configure()` if changing identity.
2. Create or select an upload keystore in Unity > Project Settings > Player > Android > Publishing Settings. Keep a private backup of the key and passwords. Enable Play App Signing in Play Console.
3. Build `Neon Orchard > Build release AAB`. This creates `Release/NeonOrchard.aab` only when custom signing is configured. Future updates need a larger version code; change the version constants in `Configure()` before building.
4. Complete the draft privacy policy's developer/contact fields and host it at a public URL. Add the URL and your support email in Play Console. Review the app's privacy text against the final binary and any SDKs added later.
5. Use `StoreListing.md`, `AppIcon-512.png`, `FeatureGraphic-1024x500.png`, and the genuine gameplay screenshots in `Screenshots/`.
6. Complete the Data safety, content rating, ads, target audience, app access and pricing forms. This version has no ads, purchases, accounts or intentional data transmission. Choose the audience based on your actual intended audience; do not assume a child-directed classification.
7. Install on Android hardware, test swipe/button controls, pause/resume, incoming interruptions, sound settings, saved progress after relaunch, tall phones, tablets, and airplane mode. Test a 16 KB page-size device/emulator and review the Play pre-launch report.
8. Upload to an internal/closed test track first. Review crashes and feedback before requesting production release.

## Verified technical settings

- Android target API: 36
- Minimum API: 26
- IL2CPP, ARM64
- Portrait UI with safe-area handling
- No ad or purchase integration
- Analytics, diagnostics and crash reporting disabled in project settings
- Pipeline automation disabled in ordinary Player builds

## Google references (checked 7 October 2026)

- [Target API requirements](https://support.google.com/googleplay/android-developer/answer/11926878): new phone apps and updates must target API 36 or higher.
- [16 KB page sizes](https://developer.android.com/guide/practices/page-sizes): check native library and package alignment and test compatible devices.
- [Store graphics](https://support.google.com/googleplay/android-developer/answer/9866151): feature graphic 1024 x 500; prepare phone screenshots and icon.
- [User data policy](https://support.google.com/googleplay/android-developer/answer/10144311): supply a privacy policy and appropriate disclosure/contact information.
- [New personal account testing](https://support.google.com/googleplay/android-developer/answer/14151465): affected personal accounts require at least 12 testers opted in continuously for 14 days before applying for production access.

Google decides review and production eligibility. Source/Editor checks are not a guarantee of acceptance, device compatibility or store performance.
