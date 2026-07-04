---
name: dotnet-android-ausbc-binding
description: Use when planning, implementing, reviewing, or validating .NET MAUI Android integration of AUSBC/UVC OTG cameras through a .NET for Android AAR binding, including Metadata.xml, api.xml, callbacks, native .so files, USB Host permissions, and MAUI service/handler boundaries.
---

# .NET Android AUSBC Binding

## Purpose

Guide project agents through SDD-ready integration of a real OTG/UVC camera in a .NET MAUI Android app using a .NET for Android binding around AUSBC `.aar` artifacts. Keep Java/Kotlin binding concerns isolated from MAUI app behavior.

## Scope Guardrails

- Do **not** edit product/application code during readiness, investigation, or planning unless the task explicitly asks for implementation.
- Prefer a separate binding project (`netX.0-android`) and a thin Android-only adapter/service consumed by MAUI.
- Do not paste proprietary vendor SDK code or assume private AUSBC APIs. Inspect the exact `.aar`/source version in the repo.
- Inter-agent communication and final model-facing requests default to English. Human-facing responses follow the human's language/style.

## Readiness Checklist

| Area | Ready when |
|---|---|
| AAR inventory | AUSBC `.aar` and all Java/Kotlin dependency AAR/JARs are identified with versions and licenses. |
| Binding project | A separate SDK-style `netX.0-android` class library contains `.aar` files as `AndroidLibrary`. |
| API surface | `obj/<Config>/api.xml` and generated C# files are inspected for key AUSBC types, listeners, builders, and lifecycle methods. |
| Metadata transforms | `Transforms/Metadata.xml` fixes only proven binding problems using XPath; `api.xml` is never edited directly. |
| Native libs | Required `.so` files and ABIs are packaged and loadable; ABI set matches target devices. |
| USB Host | Manifest declares USB Host feature/permissions strategy; runtime device permission and attach/detach flow are designed. |
| Lifecycle | Open/close/preview/record/callback cleanup maps to Android activity/fragment/MAUI lifecycle events. |
| MAUI boundary | Shared MAUI code depends on an interface; Android implementation owns Java objects, USB, views, and threading. |
| Validation | Binding builds, app packages, APK/AAB contains AAR/native assets, and hardware smoke tests are defined. |

## Binding Project Pattern

Use .NET for Android SDK-style projects. In .NET, binding projects are normal Android class libraries with binding build items.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
    <SupportedOSPlatformVersion>23</SupportedOSPlatformVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AndroidGenerateResourceDesigner>false</AndroidGenerateResourceDesigner>
  </PropertyGroup>

  <ItemGroup>
    <AndroidLibrary Include="libs\libausbc.aar" />
    <AndroidLibrary Include="libs\dependency-only.aar" Bind="false" />
  </ItemGroup>
</Project>
```

Rules:

- Use `AndroidLibrary` for `.aar`/`.jar`; set `Bind="false"` for transitive libraries not called from C#.
- Keep unintended Gradle wrapper/test jars out with `AndroidLibrary Remove="..."` if default globbing picks them up.
- Reference the binding via `ProjectReference`/`PackageReference` from the MAUI app so Java/AAR payloads flow into the final package.
- If consuming via raw `<Reference>`, the `.aar`/`.jar` must sit beside the `.dll`; avoid this unless necessary.

## Metadata.xml Transform Rules

Inspect first, transform second:

1. Build the binding with diagnostic output if needed.
2. Inspect `obj/Debug/api.xml` for package/type/member paths.
3. Inspect generated C# under `obj/Debug/generated/src` for compile errors and awkward API shapes.
4. Add minimal `Transforms/Metadata.xml` rules.
5. Rebuild and re-inspect generated C#.

Common transforms:

```xml
<metadata>
  <!-- Rename managed namespace/type/member without changing JNI identity. -->
  <attr path="/api/package[@name='com.jiangdg.ausbc']" name="managedName">Ausbc</attr>

  <!-- Remove types that cannot or should not be bound. -->
  <remove-node path="/api/package[@name='com.example.internal']" />

  <!-- Fix invalid/duplicate listener EventArgs names. -->
  <attr path="/api/package[@name='com.example']/interface[@name='CameraListener']/method[@name='on2DFrame']"
        name="argsType">CameraTwoDFrameEventArgs</attr>

  <!-- Fix mismatched managed parameter/return types. -->
  <attr path="/api/package[@name='com.example']/class[@name='Camera']/method[@name='unwrap']"
        name="managedReturn">Java.Lang.Object</attr>
</metadata>
```

Do not change Java `name` to rename a binding; use `managedName`. Direct `api.xml` edits are discarded and can break JNI registration.

## Java/Kotlin Interop Callbacks

AUSBC is Kotlin-heavy and listener/callback-driven. Verify generated bindings for:

- Kotlin companion/static members and default interface methods.
- Nested interfaces/classes and generated names.
- Listener setters converted into C# events versus raw interface implementations.
- Nullable/platform types and generic erasure that may require casts.
- Method names that become C# keywords or invalid identifiers.

Interop pattern:

- Prefer implementing generated listener interfaces in Android-specific C# classes.
- Marshal camera state and frame callbacks onto the appropriate thread before touching MAUI UI.
- Dispose Java peers (`Java.Lang.Object`, views, callbacks) when camera/session closes.
- Avoid holding stale `Activity`, `Context`, `Surface`, `TextureView`, or USB device references across lifecycle recreation.

## Native `.so` Packaging

AUSBC/libuvc may include native libraries. Verify all required ABIs (`arm64-v8a`, `armeabi-v7a`, `x86`, `x86_64`) and package them as Android native libraries when they are not already inside the AAR.

```xml
<ItemGroup>
  <AndroidNativeLibrary Include="native\arm64-v8a\libuvc.so" Abi="arm64-v8a" />
  <AndroidNativeLibrary Include="native\armeabi-v7a\libuvc.so" Abi="armeabi-v7a" />
</ItemGroup>
```

Validation:

- Inspect APK/AAB for `lib/<abi>/*.so`.
- Watch logcat for `UnsatisfiedLinkError`.
- If Java requires explicit load, call `Java.Lang.JavaSystem.LoadLibrary("name_without_lib_prefix_or_so_suffix")` before first use.

## Android USB Host Permissions

USB camera access needs Android USB Host support and a runtime `UsbManager` permission grant per device.

Manifest/readiness items:

- `uses-feature android:name="android.hardware.usb.host"` when hardware USB Host is required.
- Device filter XML if launch-on-attach is desired.
- Runtime flow for attach/detach, permission request, permission result, and denied permission.
- Graceful handling when OTG is unsupported or a device exposes unsupported UVC formats.

Do not conflate Android camera permission with UVC USB permission. AUSBC may not need `android.permission.CAMERA` for external UVC devices, but the app still needs USB Host device permission and may need audio/storage/media permissions depending on recording features and Android API level.

## AUSBC/UVC Lifecycle Model

Map AUSBC concepts to explicit app states:

1. Discover USB devices.
2. Request USB permission for selected/default device.
3. Create/render preview surface or offscreen target.
4. Build camera request (resolution, format, audio source, rotation/render mode).
5. Open/connect camera.
6. Observe `OPENED`, `CLOSED`, `ERROR`, attach/detach callbacks.
7. Capture image/video/streams only while opened.
8. Stop capture, close camera, remove callbacks, release surface/resources.

For multi-camera, keep per-device session objects. Never let one detach event close unrelated cameras.

## MAUI Integration Boundaries

Recommended layering:

| Layer | Owns | Must not own |
|---|---|---|
| Shared MAUI | Interfaces, view models, user intent, permission UX state | Java/Kotlin types, `UsbDevice`, Android views |
| Android adapter/service | Binding calls, USB permission, lifecycle, callbacks, native load | Business rules unrelated to camera |
| Handler/platform view | Preview `TextureView`/surface creation and disposal | Long-lived camera orchestration unless scoped to view lifecycle |

Use partial classes, dependency injection, or platform-specific services to expose a small C# API such as `EnumerateAsync`, `RequestPermissionAsync`, `StartPreviewAsync`, `StopAsync`, and state events. Keep raw AUSBC generated types behind Android-only code.

## Build/Test Validation

Minimum validation commands and checks:

- `dotnet build <binding>.csproj -f netX.0-android -v:minimal`
- `dotnet build <maui>.csproj -f netX.0-android -v:minimal`
- For binding failures: rebuild with binary/diagnostic log and inspect `api.xml` + `generated/src`.
- Inspect package contents for `classes.dex`, AAR resources, native `lib/<abi>/*.so`, and manifest USB declarations.
- Device smoke test with real OTG/UVC hardware: attach, grant permission, preview, switch resolution, detach during preview, reattach, app background/foreground, rotate, close.
- Capture logcat around failures; include AUSBC logs and `UnsatisfiedLinkError`, `ClassNotFoundException`, permission denial, and USB detach traces.

## Common Failure Modes

| Symptom | Likely cause | First check |
|---|---|---|
| Missing generated C# types | Missing Java dependency or non-public/obfuscated type | `api.xml`, decompiled AAR, `AndroidLibrary Bind=false/true` choices |
| Generated code does not compile | Metadata needed for duplicate names, listener args, covariant returns | `generated/src` compile error path and matching XPath |
| Runtime `ClassNotFoundException` | Dependency AAR/JAR not packaged | APK contents, `AndroidLibrary` items, transitive deps |
| Runtime `UnsatisfiedLinkError` | Missing ABI `.so` or load order issue | APK `lib/<abi>`, `AndroidNativeLibrary`, `LoadLibrary` |
| USB device visible but cannot open | Permission not granted or wrong device selected | `UsbManager.HasPermission`, permission broadcast/result, device filter |
| Preview black/frozen | Surface lifecycle mismatch or unsupported format/resolution | surface callbacks, camera request, logcat, try MJPEG/YUYV alternate |
| Crash on rotate/background | stale Activity/view/callback reference | lifecycle cleanup and re-create flow |

## Source Documentation Used

- Microsoft Learn: Xamarin.Android binding project migration for .NET MAUI — `AndroidLibrary`, default file inclusion, .NET binding project model.
- Microsoft Learn: Java Bindings Metadata — `api.xml`, `Transforms/Metadata.xml`, `managedName`, `remove-node`, `argsType`, `managedReturn`, XPath rules.
- Microsoft Learn: Troubleshooting Bindings — dependency inclusion, diagnostic logs, generated C# inspection, native `.so` loading.
- Microsoft Learn: .NET for Android Build Items — `AndroidLibrary`, `AndroidNativeLibrary`, `AndroidManifestOverlay`, Maven/Gradle-related build items.
- Android Developers: USB Host concepts — USB Host/device permission, device filters, attach/detach model.
- AUSBC GitHub README (`jiangdongguo/AndroidUSBCamera`) — AUSBC features, Kotlin usage model, camera request/lifecycle callbacks, multi-camera flow, supported ABIs.
