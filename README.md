# Neon Orchard

A complete offline portrait Snake game built with Unity 6000.3.10f1, uGUI, TextMeshPro and the Input System.

Open `Assets/NeonOrchard/Scenes/Orchard.unity` and press Play. Swipe with a finger or mouse, use the on-screen direction buttons, or use arrow keys/WASD. Escape pauses or goes back.

## Game

- 24 progressively harder stages, four chapters, star ratings and level unlocks
- Endless mode with increasing speed and local high scores
- Four colors earned by collecting fruit
- Music, effects, optional vibration, pause/resume and automatic background pause
- Safe-area-aware layout, local save repair, no ads or purchases
- Original procedural graphics and composed synthesized audio

## Code

`SnakeModel.cs` holds deterministic grid rules and stage definitions. `OrchardApp.cs` constructs the functional UI and handles saves/input. `OrchardGraphics.cs` draws the board and rounded UI shapes. `OrchardAudio.cs` composes the music and effects. `Editor/OrchardBuilder.cs` contains reproducible scene setup, build methods and verification routines.

The `Neon Orchard` menu includes Android test APK and release AAB builds. The release build requires an upload keystore already configured in Player Settings > Publishing Settings. Building the test APK selects the default debug key; reselect your upload key before a store release.

## Android

Target API 36, minimum API 26, IL2CPP, ARM64, portrait, version 1.0.0 (code 1). Initial application ID: `com.neonorchard.snake`. Confirm ownership and desired ID before first Play Console upload; subsequent releases must keep the same ID. `Configure()` in OrchardBuilder owns these settings, so update its constants when changing the release identity/version.

Player analytics, diagnostics and crash reporting are disabled. Pipeline is used for editor automation and defaults off in Player builds.

## Verification

`Release/GameplayTests.txt` records 192 successful stage playthroughs, using eight food seeds for every stage, plus movement/collision/save checks. `Release/UiTests.txt` records UI flow verification. To rerun: evaluate `NeonOrchard.Editor.OrchardBuilder.Tests()` in Edit mode and `NeonOrchard.Editor.OrchardBuilder.SmokeTests()` in Play mode through Unity Pipeline.

Phone hardware and Play pre-launch testing are still necessary before publication. Editor tests do not establish Android hardware performance, audio latency, vibration behavior, or Play acceptance.

## Release material

See `Release/StoreListing.md`, screenshots, icon, feature graphic, draft privacy policy and release checklist. Secrets and signing files belong outside source archives and must be backed up by the publisher. No publication is performed by the project scripts.
