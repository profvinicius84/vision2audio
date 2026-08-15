# AUSBC / AndroidUSBCamera binding notes

The Android phone does not expose the USB camera via Camera2, so OTG/UVC must be handled as USB Host + UVC.

## Current scaffold

- `src/Vision2Audio.AusbcBinding/` — .NET Android binding project targeting `net10.0-android`.
- `external/ausbc/` — AUSBC `.aar` artifacts consumed by the binding project. The binding now prefers compatible AndroidUSBCamera `3.2.9` dependency AARs extracted from `AndroidUSBCamera-3.3.3.zip` when present.
- `src/Vision2Audio.App/Platforms/Android/UsbCameraService.cs` — current USB Host diagnostics and permission request boundary.

## Current planning status

- The broad AUSBC task has been superseded by Wave 2: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`.
- Execution should follow tasks `vision2audio-2.1` through `vision2audio-2.6` under `docs/superpowers/tasks/`.
- Camera selection has precedence over the older OTG-first default. AUSBC is used when OTG is selected and available; fallback must be visible when OTG is unavailable or fails.

## Task `vision2audio-2.2` binding inspection result

Validation command run on 2026-07-03:

```text
dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android
```

Result: build succeeded with 9 generated-code warnings and 0 errors. Warnings were limited to generated binding code: hidden inherited members (`CS0114`, `CS0108`), protected members on sealed generated types (`CS0628`), obsolete generated `ICameraStrategy` usage (`CS0618`), nullable mismatch (`CS8767`), and one generated nullable literal warning (`CS8625`). No metadata transform was required for compilation.

Generated files inspected:

- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/api.xml`
- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.CameraClient.cs`
- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.MultiCameraClient.cs`
- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.Camera.CameraUVC.cs`
- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.Camera.ICameraStrategy.cs`
- `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.Camera.Bean.CameraRequest.cs`
- callback bindings under `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.Callback.*.cs`
- preview view binding `src/Vision2Audio.AusbcBinding/obj/Debug/net10.0-android/generated/src/Com.Jiangdg.Ausbc.Widget.AspectRatioTextureView.cs`

## Generated API surface discovered

Primary USB/permission/session entry point:

- `Com.Jiangdg.Ausbc.MultiCameraClient`
  - Constructor: `MultiCameraClient(Android.Content.Context ctx, Com.Jiangdg.Ausbc.Callback.IDeviceConnectCallBack? callback)`
  - USB lifecycle: `Register()`, `UnRegister()`, `Destroy()`
  - USB permission: `HasPermission(Android.Hardware.Usb.UsbDevice?)`, `RequestPermission(Android.Hardware.Usb.UsbDevice?)`

Device connection callbacks:

- `Com.Jiangdg.Ausbc.Callback.IDeviceConnectCallBack`
  - `OnAttachDev(Android.Hardware.Usb.UsbDevice)`
  - `OnDetachDec(Android.Hardware.Usb.UsbDevice)`
  - `OnCancelDev(Android.Hardware.Usb.UsbDevice)`

Primary UVC camera/session type:

- `Com.Jiangdg.Ausbc.Camera.CameraUVC : Com.Jiangdg.Ausbc.MultiCameraClient.ICamera`
  - Constructor: `CameraUVC(Android.Content.Context ctx, Android.Hardware.Usb.UsbDevice device)`
  - Open/close via inherited public API: `OpenCamera(Java.Lang.Object? cameraView, CameraRequest? cameraRequest)`, `CloseCamera()`
  - Capture: `CaptureImage(ICaptureCallBack callBack, string? path)`
  - Preview frames: `AddPreviewDataCallBack(IPreviewDataCallBack callBack)`, `RemovePreviewDataCallBack(IPreviewDataCallBack callBack)`
  - State callback: `SetCameraStateCallBack(ICameraStateCallBack? callback)`
  - Sizing/state helpers: `GetAllPreviewSizes(Java.Lang.Double? aspectRatio)`, `GetSuitableSize(int maxWidth, int maxHeight)`, `IsCameraOpened`, `UsbDevice`, `UpdateResolution(int width, int height)`

Camera request builder:

- `Com.Jiangdg.Ausbc.Camera.Bean.CameraRequest.Builder`
  - `SetPreviewWidth(int)`, `SetPreviewHeight(int)`
  - `SetRawPreviewData(bool)`, `SetCaptureRawImage(bool)`
  - `SetAspectRatioShow(bool)`
  - `SetAudioSource(CameraRequest.AudioSource)` with enum values `None`, `SourceAuto`, `SourceDevMic`, `SourceSysMic`
  - `SetRenderMode(CameraRequest.RenderMode)` with enum values `Normal`, `Opengl`
  - `Create()`

Preview view/surface options:

- `Com.Jiangdg.Ausbc.Widget.AspectRatioTextureView : Android.Views.TextureView, IAspectRatio`
  - Constructors accepting `Android.Content.Context`
  - `Surface`, `SurfaceWidth`, `SurfaceHeight`, `SetAspectRatio(int width, int height)`
- Also generated: `AspectRatioSurfaceView`, `AspectRatioGLSurfaceView`, and `IAspectRatio`.

Capture callback:

- `Com.Jiangdg.Ausbc.Callback.ICaptureCallBack`
  - `OnBegin()`
  - `OnComplete(string? path)`
  - `OnError(string? error)`

Preview data callback:

- `Com.Jiangdg.Ausbc.Callback.IPreviewDataCallBack`
  - `OnPreviewData(byte[]? data, int width, int height, IPreviewDataCallBack.DataFormat format)`
  - Use with `CameraRequest.Builder.SetRawPreviewData(true)` when downstream code needs frame bytes.

Camera state callback:

- `Com.Jiangdg.Ausbc.Callback.ICameraStateCallBack`
  - `OnCameraState(MultiCameraClient.ICamera self, ICameraStateCallBack.State code, string? msg)`
  - State enum values exposed as `Opened`, `Closed`, `Error`.

Legacy/deprecated-but-generated API:

- `Com.Jiangdg.Ausbc.CameraClient` and `Com.Jiangdg.Ausbc.Camera.ICameraStrategy` are generated and usable at compile time but marked obsolete/deprecated in generated C#. Prefer `MultiCameraClient` + `CameraUVC` for the Android USB/UVC boundary in later tasks.

## Native library inventory

Current `.aar` artifacts under `external/ausbc/` expose these native libraries:

- `libnative-3.2.9.aar`: `jni/arm64-v8a/libnativelib.so`, `jni/armeabi-v7a/libnativelib.so`
- `libuvc-3.2.9.aar`: `jni/arm64-v8a/libUACAudio.so`, `libUVCCamera.so`, `libjpeg-turbo1500.so`, `libusb100.so`, `libuvc.so`; same set under `jni/armeabi-v7a/`, `jni/x86/`, and `jni/x86_64/`
- `libutils-3.2.9.aar` and `libuvccommon-3.2.9.aar`: dependency AARs from `AndroidUSBCamera-3.3.3.zip`, included as dependency-only `AndroidLibrary` items.
- Legacy `libnative-release.aar` and `libuvc-release.aar` remain in `external/ausbc/`, but the binding project uses them only when the compatible `3.2.9` artifacts are absent.
- `libausbc-release.aar` and `xlog-1.11.0.aar`: no native `.so` entries found in the AAR listing.

No APK/AAB packaging inspection was performed in task `vision2audio-2.2`; that belongs to consuming-app tasks after the binding is referenced by the MAUI app.

## Limitations and risks

- The binding compiles, but real OTG/UVC readiness is not proven by this task. Hardware validation on Android 11 remains pending for later Wave 2 tasks.
- The generated APIs use raw Java/Kotlin types (`UsbDevice`, `Java.Lang.Object`, generated callbacks). Later MAUI code must keep these behind Android-only adapter/service boundaries.
- `OpenCamera(Java.Lang.Object? cameraView, CameraRequest? cameraRequest)` accepts a generic Java object. Later implementation should pass a generated AUSBC preview view such as `AspectRatioTextureView`/`IAspectRatio` and verify runtime behavior on hardware.
- Still capture writes to a path and reports completion through `ICaptureCallBack.OnComplete(path)`; downstream capture routing must handle temp file lifecycle and cleanup.
- The compatible `libuvc-3.2.9.aar` includes native libraries for `arm64-v8a`, `armeabi-v7a`, `x86`, and `x86_64`; real emulator behavior still requires validation because USB/OTG availability differs by emulator/host.
- Android 16 / 16 KB native page-size readiness is not claimed. The build command for this binding did not emit an Android 16 page-size warning, but the bundled native `.so` files have not been independently verified for 16 KB page-size compatibility.
- No secrets, device serials, full environment dumps, or logcat output were added to this document.
- Release packaging blocker resolved on 2026-07-03: Xlog remains packaged as an AUSBC Java dependency, but its managed binding API surface is excluded by metadata because it is dependency-only for this app. The prior `CS0535` generated formatter errors are no longer emitted in Release binding/app publish.

## Next integration steps

1. Preserve native/emulator fallback when OTG/UVC is unavailable, denied, or unsupported.
2. Validate on Android 11 hardware with OTG/UVC attach, permission grant, preview/open, capture, detach/reattach, background/foreground, rotation, and fallback checks before claiming real-camera readiness.

## Task `vision2audio-2.3` session boundary result

- `UsbCameraService.InitializeAsync` now keeps the Android-only AUSBC boundary inside platform code: it discovers a UVC candidate, requests USB Host permission, registers `MultiCameraClient`, creates `CameraUVC`, and opens a minimal UVC session with `CameraRequest.Builder`.
- Preview and still-capture routing remain intentionally unsupported pending task `vision2audio-2.4`; `CameraPreviewViewHandler` and native Camera2 fallback paths were not changed.
- `IUsbCameraService.CloseSessionAsync` was added so Android platform callers can close the active UVC session and release AUSBC resources.
- Detach handling uses an Android USB detach receiver to close only the active session; diagnostics avoid full `UsbDevice.ToString()` and USB serial fields.
- Build validation on 2026-07-03 succeeded for `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`; core tests passed. The APK contains `classes.dex`, `AndroidManifest.xml`, and AUSBC native libraries under `lib/arm64-v8a/`.
- Android 11 OTG/UVC hardware validation is still pending; do not claim real-camera readiness until permission, open/close, detach/reattach, and fallback behavior are verified on device.

### Follow-up hardening applied during blocker resolution

- Android 13+ dynamic USB receivers are registered with `ReceiverFlags.NotExported`; Android 12 and earlier do not support this flag for dynamic receiver registration.
- The custom USB permission `PendingIntent` is package-scoped, one-shot, and validated with a per-request random nonce. Because adding an app-defined receiver permission would also block Android's system USB permission result sender, the final safety boundary before opening AUSBC is an explicit `_usbManager.HasPermission(device)` re-check after the permission await and immediately before session open.
- `secrets.local.json` remains available as a local development file but is excluded from default app package assets. Packaging it now requires explicit MSBuild opt-in with `IncludeLocalSecretsInAppPackage=true`.

## Task `vision2audio-2.4` preview/capture routing result

- `CameraPreviewViewHandler` routes `CameraSelectionKind.Otg` to the singleton Android `IUsbCameraService` instead of Camera2 external-camera probing. Front/rear selections still use the existing native Camera2 preview path.
- `UsbCameraService.StartPreviewAsync(TextureView, ...)` keeps the selected OTG device, USB permission flow, AUSBC `CameraUVC`, and preview `TextureView` in one Android-only session boundary.
- `UsbCameraCaptureService` now captures through `IUsbCameraService.CaptureFrameAsync`, which snapshots the same AUSBC-backed preview `TextureView`; it no longer opens a separate Camera2 external still-capture session.
- `CameraSourceCoordinator` stops the previously active preview source before starting a new selected/fallback source, reducing stale-session risk when switching between OTG and native sources.
- Validation on 2026-07-03: Android app build succeeded for `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`; core tests passed with 10/10 tests. APK inspection found `classes.dex`, `AndroidManifest.xml`, and arm64 AUSBC native libraries (`libnativelib.so`, `libjpeg-turbo1500.so`, `libusb100.so`, `libuvc.so`, `libUVCCamera.so`).
- Android 11 OTG/UVC hardware validation remains pending; runtime behavior of AUSBC `OpenCamera(TextureView, CameraRequest)` must be confirmed on device before closing real-camera readiness.

## Task `vision2audio-2.6` validation evidence

Validation commands run on 2026-07-03:

```text
dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android
dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android
dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj
dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
adb devices
```

Results:

- Debug AUSBC binding build succeeded with 9 generated-code warnings and 0 errors.
- Debug MAUI Android app build succeeded with 0 warnings and 0 errors.
- Core tests passed: 11 passed, 0 failed, 0 skipped.
- Release APK publish initially failed in generated `Com.Elvishew.Xlog.*` binding code with `CS0535` formatter interface errors. This reproduced the known Release AUSBC binding risk and blocked APK acceptance evidence until the metadata fix described below.
- `adb` was not installed/on PATH in the validation environment, so emulator enumeration and manual emulator fallback validation were not executed.
- Android 11 Xiaomi OTG/UVC hardware validation was not executed because the validation environment has no physical device/camera access. Do not claim real OTG/UVC preview/capture acceptance from this evidence.
- Security review found no app source logging of OpenAI key values, USB serial fields, full environment dumps, image payload contents, or captured image bytes. A local `secrets.local.json` with an OpenAI-key-shaped value exists in the workspace and is ignored by `.gitignore`; do not commit it, and rotate the key if the workspace/output is considered exposed.

## Wave 2 Release APK binding blocker resolution

Validation commands run on 2026-07-03 after the metadata fix:

```text
dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android -c Release
dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android
dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android
dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj
```

Results:

- Release AUSBC binding build succeeded with generated binding warnings and 0 errors.
- Release APK publish succeeded; output includes `bin/Release/net10.0-android/android-arm64/publish/com.companyname.vision2audio.app-Signed.apk`.
- Debug AUSBC binding build and Debug MAUI Android app build succeeded with generated binding warnings and 0 errors.
- Core tests passed: 11 passed, 0 failed, 0 skipped.
- APK inspection found `classes.dex` and arm64 AUSBC native libraries: `libnativelib.so`, `libjpeg-turbo1500.so`, `libusb100.so`, `libuvc.so`, and `libUVCCamera.so`.
- APK secret asset check found 0 `secrets.local.json` entries and 0 asset names matching `openai`, `secret`, `apikey`, or `api_key`.

Root cause and fix:

- In Release binding generation, the Xlog AAR was present in `class-parse.rsp` and contributed `com.elvishew.xlog.formatter.*` generic formatter APIs to `api.xml`; generated concrete formatter classes exposed typed `Format(...)` overloads but inherited a managed non-generic `IFormatter.Format(Object?)` contract, causing `CS0535`.
- Xlog is not called from C#; it is only required as a Java runtime dependency for AUSBC/libuvc logging. `Transforms/Metadata.xml` now removes the managed `com.elvishew.xlog*` API surface while keeping the AAR packaged via `AndroidLibrary Bind="false"`.

Remaining risks:

- Generated binding warnings remain (`BG8401`, `BG8403`, `CS0108`, `CS0114`, `CS0618`, `CS0628`, `CS8625`, `CS8767`) and should be revisited only if they affect consumed AUSBC APIs.
- Release publish emits Android 16 / 16 KB page-size warnings for bundled AUSBC native libraries; Android 11 OTG/UVC validation remains pending.

## Android 11 OTG/UVC resolution blocker fix

Human validation reported a physical OTG/UVC camera that only exposes 640x480 and fails around preview resolution setup.

Root cause found in code inspection:

- The AUSBC session already requested 640x480, but it passed a plain Android `TextureView` to `CameraUVC.OpenCamera(...)` while `CameraRequest.SetAspectRatioShow(true)` expects an AUSBC aspect-ratio-capable preview view for sizing.
- The preview surface/buffer was not explicitly configured to the selected UVC resolution before opening the camera, so AUSBC could see a view/surface size different from the requested 640x480 stream.
- The session did not use the exposed AUSBC preview-size helpers before building the request.

Fix applied:

- The Android preview handler now creates `Com.Jiangdg.Ausbc.Widget.AspectRatioTextureView`, which remains a `TextureView` subclass for the existing native Camera2 path but also satisfies AUSBC `IAspectRatio` behavior for OTG.
- `UsbCameraService` prefers 640x480, verifies it with `IsPreviewSizeSupported(...)` when AUSBC exposes the information, falls back to `GetSuitableSize(...)` if exact 640x480 is not reported, and finally keeps 640x480 as the safe known target if size-helper calls are not available before open.
- Before `OpenCamera(...)`, the service applies the chosen size to the AUSBC aspect-ratio view and to the `SurfaceTexture` default buffer size, then builds the `CameraRequest` with the same width/height.

Validation run after the fix:

- `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android` succeeded with existing generated binding warnings and known Android 16 native page-size warnings.

Android 11 OTG/UVC hardware validation is still required: attach the 640x480 camera, grant USB permission, select OTG, verify preview opens, capture uses the OTG frame, detach/reattach recovers or falls back visibly, and native front/rear camera preview still works.

## Android 11 OTG/UVC USBMonitor class-resolution blocker

Human validation later reported this runtime fallback message when selecting OTG:

```text
Otg: OTG indisponível: Failed resolution of: Lcom/jiangdg/usb/USBMonitor;
```

Initial investigation result:

- `libausbc-release.aar` contains AUSBC classes that reference `com/jiangdg/usb/USBMonitor`.
- The current `external/ausbc/libuvc-release.aar` packages `com/serenegiant/usb/USBMonitor` instead.
- None of the current artifacts under `external/ausbc/` define `com/jiangdg/usb/USBMonitor.class`.
- APK dex inspection confirmed `com.serenegiant.usb.USBMonitor` is defined, while `com.jiangdg.usb.USBMonitor` is only referenced and not defined.

Root cause: the AUSBC and UVC dependency AARs were version/package mismatched. This was not a C# metadata issue and could not be fixed by `Transforms/Metadata.xml`; the correct Java dependency artifact was missing from the repository.

Fix applied after `AndroidUSBCamera-3.3.3.zip` was added to the project root:

- Extracted compatible AARs to `external/ausbc/`: `libuvc-3.2.9.aar`, `libnative-3.2.9.aar`, `libutils-3.2.9.aar`, and `libuvccommon-3.2.9.aar`.
- Confirmed `libuvc-3.2.9.aar` contains `com/jiangdg/usb/USBMonitor.class`.
- Updated `Vision2Audio.AusbcBinding.csproj` to prefer `libuvc-3.2.9.aar` and `libnative-3.2.9.aar`, with fallback to the older release artifacts only if the compatible files are absent.
- Included `libutils-3.2.9.aar` and `libuvccommon-3.2.9.aar` as dependency-only `AndroidLibrary` items with `Bind="false"`.

Validation after the fix:

- `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android` succeeded with generated binding warnings and 0 errors.
- `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` passed 11/11.
- A clean `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk` succeeded.
- APK DEX inspection found `Lcom/jiangdg/usb/USBMonitor;` in `classes2.dex` and did not find `Lcom/serenegiant/usb/USBMonitor;`.
- APK secret asset inspection found no `secrets.local.json` or secret-like asset names.

Validation status in this environment: the missing `USBMonitor` class packaging blocker is resolved. Android 11 OTG/UVC runtime validation on physical hardware remains required before acceptance.

## AndroidUSBCamera 3.6.0 update evaluation

`AndroidUSBCamera-3.6.0.zip` in the project root was inspected for a USB camera library update.

Findings:

- The zip contains source for `com.jiangdg.ausbc.*`, including `CameraUVC`, `ICaptureCallBack`, `CameraRequest`, `AspectRatioTextureView`, and `com.jiangdg.usb.USBMonitor`.
- It contains prebuilt dependency AARs only for `libnative/aar/libnative-3.2.9.aar` and `libuvc/aar/libuvc-3.2.9.aar`.
- It does not contain a prebuilt `libausbc-release.aar`, which is the primary AAR consumed by the current binding for `CameraUVC` and the generated AUSBC API surface.
- The packaged `libnative-3.2.9.aar` and `libuvc-3.2.9.aar` have the same byte length and SHA-256 hashes as the already-present files in `external/ausbc/`, so copying them would not change runtime artifacts.

Migration decision:

- Full migration to 3.6.0 is blocked until a matching `libausbc` AAR is built or supplied.
- The binding remains on the current compatible `libausbc-release.aar` plus the `3.2.9` dependency AARs; no duplicate old/new AAR set was introduced.

Capture timeout root-cause update from 3.6.0 source:

- `CameraUVC.captureImageInternal(savePath, callback)` waits for `mNV21DataQueue.pollFirst(...)` and reports `Times out` if no frame data arrives.
- `mNV21DataQueue` is filled from AUSBC's `IFrameCallback`.
- In OpenGL render mode, `CameraUVC.openCameraInternal(...)` registers that frame callback only when `CameraRequest.isRawPreviewData` or `CameraRequest.isCaptureRawImage` is true.
- The app previously used OpenGL render mode but set both flags to false, so still capture could time out even when preview was open.

Fix applied:

- `UsbCameraService.CreateSessionRequest(...)` now sets both `SetRawPreviewData(true)` and `SetCaptureRawImage(true)` for OTG sessions, while preserving OpenGL render mode and same-source OTG capture behavior.
- OTG active capture still fails explicitly if AUSBC capture fails; it does not silently fall back to native camera.

## Android 11 OTG/UVC storage permission report

After raw preview/capture flags were enabled, human validation reported an AUSBC/storage permission style failure during OTG capture.

Source inspection result:

- `CameraUVC.captureImageInternal(savePath, callback)` does not call `hasStoragePermission()`; it writes the supplied `savePath` through `MediaUtils.saveYuv2Jpeg(...)`.
- `MediaUtils.saveYuv2Jpeg(...)` uses `FileOutputStream(File(path))` and returns false on `IOException`.
- AUSBC default directories use `ctx.getExternalFilesDir(Environment.DIRECTORY_DCIM)/Camera`, which is app-private external storage and should not require broad storage permission on Android 11.
- Legacy `Camera1Strategy`, `Camera2Strategy`, and `CameraUvcStrategy` check `WRITE_EXTERNAL_STORAGE`, but the currently used `CameraUVC` path does not.

Fix applied:

- The app now creates OTG still-capture files under app-private external pictures storage first: `Context.GetExternalFilesDir(Environment.DirectoryPictures)/otg-captures`.
- It falls back to external cache, then internal cache if app-private external storage is unavailable.
- Before calling AUSBC, it creates the directory and performs a one-byte write/delete probe, logging only the storage category and writable status, never the full path.
- No `MANAGE_EXTERNAL_STORAGE`, media permission, or broad storage runtime permission was added.
