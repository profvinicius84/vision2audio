# Stack Context

## Technologies

- .NET MAUI
- Android
- OpenAI API (direct from client)

## Frameworks and libraries

- MAUI application stack

## Tooling

- Repository platform: GitHub.
- AI platform: OpenAI.
- Model routing policy: to be defined by Tech Lead.
- Task-management platform: pending decision.

## Runtime and environment assumptions

- Runs on Android devices.
- Uses an OTG camera and GPS-capable phone hardware.
- Requires internet connectivity.
- Direct OpenAI integration is approved; setup details still need platform and routing confirmation.

## Validation commands

- `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Android 11 device validation remains required for OTG camera hardware.
- Main-screen preview panel uses the camera-source coordinator and must stay synchronized with capture.
- AUSBC binding build for real OTG/UVC work: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android`
- APK packaging validation for camera hardening: `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
- 2026-07-03 validation: AUSBC Debug binding build succeeded with generated-code warnings; app Debug build succeeded; core tests passed 11/11; Release APK publish initially failed in generated AUSBC/Xlog formatter binding code (`CS0535`) but was fixed by excluding dependency-only Xlog managed API surface in `Transforms/Metadata.xml`; Release publish now succeeds.
- 2026-07-03 follow-up: `AndroidUSBCamera-3.3.3.zip` supplied compatible `libuvc-3.2.9.aar` with `com/jiangdg/usb/USBMonitor.class`; binding now prefers the compatible 3.2.9 AARs and Release APK DEX inspection confirmed `Lcom/jiangdg/usb/USBMonitor;` is present.
- 2026-07-04 diagnostic follow-up: targeted sanitized OTG/AUSBC diagnostics and best-effort cleanup handling were added after device output showed only `System.InvalidOperationException`; core tests now pass 14/14, binding/app builds pass, and Release APK publish passes. Binding packaging excludes stale legacy native/UVC AARs when compatible 3.2.9 AARs exist to avoid duplicate Java `BuildConfig` classes.
- 2026-07-04 OTG capture follow-up: capture no longer reads `TextureView.Bitmap`; it uses AUSBC `CameraUVC.CaptureImage(ICaptureCallBack, savePath)` to app-private cache and reads JPEG bytes. Core tests pass 14/14, binding/app builds pass, and Release APK publish passes.
- 2026-07-04 secrets/capture-timeout follow-up: app supports `secrets.local.json` and `secrets.local` as Debug/explicit local assets but keeps them out of Release APK by default; Release APK secret-entry inspection passed. AUSBC request render mode changed to OpenGL and capture diagnostics were added for timeout analysis. `AndroidUSBCamera-3.6.0.zip` exists in the root but the binding still uses the `3.2.9` AAR set pending manual retest/evaluation.
- 2026-07-04 priority UI/capture follow-up: Volume Up trigger, captured-image freeze, second-capture recovery, scrollable main screen, and visible text cleanup were implemented. Core tests pass 14/14 and Release APK publish passes; Debug build can be blocked by Visual Studio Remote Debugger file locks.
- 2026-07-04 app polish/accessibility follow-up: MAUI icon uses root `logo.jpeg`; AI loading, TTS interruption/resume-preview lifecycle, GPS-unavailable-tolerant coordinator behavior, and visually-impaired navigation/location prompt updates were implemented. Core tests pass 14/14, Android app build passes, and Release APK publish passes with existing AUSBC Android 16 page-size warnings.
- 2026-07-04 location correction: MAUI reverse geocoding converts GPS coordinates into a human-readable approximate address/place for the OpenAI prompt; prompt instructs the model not to mention latitude/longitude. Core tests, Android build, and Release APK publish pass.
- Android device/emulator validation requires Android SDK tooling (`adb`) and attached hardware/emulator; neither was available in the 2026-07-03 validation environment.
