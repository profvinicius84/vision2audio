# Task: Validate Android 11 hardware, emulator fallback, and acceptance evidence

## Metadata

- Task ID: `vision2audio-2.6`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-27-vision-2-audio-design; 2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `blocked`
- Size: `M`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Acceptance 66-88; Context updates 133-143`
- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Acceptance 58-76; Context updates 114-120`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec reference: `Acceptance 59-77; Context updates 116-122`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.6`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Collect final validation evidence for Wave 2 across build, unit tests, APK publish, emulator fallback, Android 11 OTG/UVC hardware behavior, acceptance criteria, and documentation updates.

## Expected result

Wave 2 has concrete evidence for pass/fail/deferred gates, and current context accurately reflects the validated implementation state.

## In scope

- Run required build, test, and package commands — source: `.opencode/context/stack.md` and Wave 2.
- Validate Android 11 OTG/UVC preview and capture on target hardware — source: `MVP and camera specs`.
- Validate emulator fallback when OTG is unavailable — source: `preview and selection specs`.
- Validate selected-source changes update preview and capture together — source: `selection spec`.
- Update durable docs/context with evidence and remaining limitations — source: context update sections in specs.

## Out of scope

- New implementation changes except small documentation corrections.
- Claiming acceptance without evidence or explicit deferral.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `testing-specialist`: command and device validation evidence.
- `acceptance-specialist`: criteria-by-criteria review.
- `cybersecurity-specialist`: final security gate.
- `docs-maintainer.md` or `context-maintainer.md`: durable documentation updates.

### Recruited agents

- None.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `verification-before-completion` | `testing-specialist`, `acceptance-specialist` | Evidence-first closure. |
| `systematic-debugging` | `testing-specialist` | Triage validation failures. |
| `cybersecurity` | `cybersecurity-specialist` | Final permissions/logging/secret review. |

## Files or areas expected to change

- `.opencode/context/current-state.md`
- `.opencode/context/architecture.md`
- `.opencode/context/decisions.md`
- `.opencode/context/stack.md`
- `docs/ausbc-binding.md`
- Wave/task completion notes or validation evidence artifacts if produced

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android`
- Build/check command: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Test command: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- Package command: `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
- Manual validation: Android 11 Xiaomi device with OTG/UVC camera attached; grant USB permission; select OTG; verify preview starts; capture uses OTG frame; detach shows fallback/alert; reattach can recover; native front/rear still work.
- Manual validation: emulator without OTG remains usable and clearly falls back.
- Security validation: confirm no OpenAI key, USB serial, full environment dump, image payload, or sensitive path is logged.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `blocked` | Diagnostic/error-handling review blockers were fixed and automated review gate passed; manual Android 11 hardware and emulator validation still prevent task closure. |
| tests | `passed` | Binding build, app build, core tests, clean Release APK publish, secret asset inspection, APK DEX inspection, and follow-up diagnostic-handler validation passed after Xlog metadata, AndroidUSBCamera 3.2.9 dependency, cleanup-safety, and duplicate-AAR fixes. |
| acceptance | `blocked` | Android 11 OTG/UVC and emulator fallback validation were not executed in this environment. |
| security | `passed` | App source/package review did not find prohibited logging or packaged local secrets; local `secrets.local.json` remains an operational risk and must stay uncommitted/rotated if exposed. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Physical OTG hardware is unavailable | Human / Testing specialist | Record explicit hardware-validation deferral; do not close OTG acceptance as passed. |
| Android 11 OTG/UVC preview reported a resolution-related runtime error with a 640x480 camera | AUSBC Android Binding Specialist / Human tester | Code now aligns AUSBC request, aspect-ratio view, and surface buffer to 640x480 when available; retest on hardware with the latest APK. |
| Android 11 OTG selection failed with `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;` | AUSBC Android Binding Specialist / Human tester | Packaging blocker resolved: compatible `libuvc-3.2.9.aar` is included and clean Release APK DEX inspection found `Lcom/jiangdg/usb/USBMonitor;`. Retest on physical Android 11 hardware to confirm runtime behavior. |
| Android 11 validation later emitted `System.InvalidOperationException` without message/stack in Visual Studio output | MAUI Android Stack Specialist / Human tester | Targeted sanitized diagnostics were added around OTG/AUSBC preview/capture and lifecycle cleanup. Retest with the latest APK and collect `[Diagnostics]` lines if the exception recurs. |
| Android 11 OTG capture failed with `Prévia OTG/AUSBC indisponível para captura.` | AUSBC Android Binding Specialist / Human tester | Root cause confirmed: capture still depended on `_previewView.Bitmap`, but the preview `TextureView` was unavailable at capture time. Capture now uses AUSBC `CameraUVC.CaptureImage(ICaptureCallBack, savePath)` to an app-private cache file, reads JPEG bytes, and deletes the temp file best-effort. Retest on physical Android 11 hardware. |
| Android 11 OTG capture failed with `Captura OTG/AUSBC excedeu o tempo limite.` | AUSBC Android Binding Specialist / Human tester | Current follow-up added sanitized `otg-capture-*` diagnostics and switched AUSBC request render mode to OpenGL. Retest required to confirm whether `CaptureImage` callbacks now fire. |
| App stopped reading `secrets.local` | MAUI Android Stack Specialist / Human tester | Local secret loading now supports both `secrets.local.json` and `secrets.local` as explicitly packaged Debug/local assets, while Release packaging verification confirmed no `secrets.local*` or secret-like APK entries. |
| Android SDK `adb` unavailable | Human / Testing specialist | Install/configure Android SDK platform tools or run validation in an environment with emulator/device access. |
| Android 16 native library warnings remain | Testing specialist | Record as known limitation, not a Wave 2 blocker for Android 11. |

## Completion report

- What changed: Documentation/evidence only. No feature implementation changes were made.
- Agents used: `testing-specialist` role in this session.
- Skills used: `verification-before-completion`, `csharp-xunit`, `systematic-debugging`, `cybersecurity`.
- Validation executed on 2026-07-03:
  - `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android` — succeeded; 9 generated binding warnings, 0 errors.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android` — succeeded; 0 warnings, 0 errors.
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` — passed; 11 passed, 0 failed, 0 skipped.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk` — initially failed in `Vision2Audio.AusbcBinding` generated Release binding code with Xlog formatter interface errors (`CS0535`), then passed after excluding the dependency-only Xlog managed API surface via `Transforms/Metadata.xml`.
  - Post-fix validation: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android -c Release` passed; Release APK publish passed; APK secret asset check found 0 `secrets.local.json` entries and 0 secret-like asset names.
  - `adb devices` — not executable in this environment because `adb` is not installed/on PATH.
  - Security source/logging review — app source uses startup/status `Debug.WriteLine` entries and does not show logging of OpenAI API key values, USB serials, full environment dumps, image payload contents, or captured image data. A local `secrets.local.json` containing an OpenAI-key-shaped value exists in the workspace and is ignored by `.gitignore`; do not commit it, and rotate the key if this workspace/output is considered exposed.
- Validation not executed and why:
  - Android 11 Xiaomi OTG/UVC hardware validation was not executed because no physical device/camera access is available from this environment.
  - Emulator fallback validation was not executed because Android SDK `adb` was unavailable from this environment and no emulator/device could be enumerated.
- Context updates completed or needed: `current-state.md`, `stack.md`, and `docs/ausbc-binding.md` updated with validation evidence/blockers. Architecture and decisions did not need behavior updates because product behavior did not change.
- Documentation updates completed or needed: This task file and AUSBC binding notes now record the Release publish fix and manual validation deferral.
- Remaining risks: Android 11 OTG/UVC behavior remains unaccepted; emulator fallback remains unverified in this environment; local secret file must remain uncommitted and may need rotation if exposed; Android 16 16 KB page-size warnings remain for bundled AUSBC native libraries.
- Recommended next step: Keep task/wave blocked only on manual validation. Provide an environment with Android SDK/emulator access and Android 11 OTG/UVC hardware, then rerun task 2.6 manual acceptance checks before claiming Wave 2 acceptance.

## Follow-up diagnostic/error-handler addition

- Human report: after the `USBMonitor` fix, device output showed AUSBC OTG startup progress but later emitted only `Exception thrown: 'System.InvalidOperationException' in Vision2Audio.App.dll` without message or stack trace. ADB was unavailable on the user's machine.
- Fix applied: added targeted sanitized diagnostics around OTG/AUSBC preview, capture, platform preview restart, and cleanup boundaries. Diagnostics include operation name, exception type, sanitized message, and sanitized stack trace where available.
- Cleanup behavior: preview-source cleanup, preview-restart cleanup, and fire-and-forget lifecycle cleanup are now best-effort and sanitized-logged so cleanup failures do not mask the original OTG failure or produce unobserved task faults.
- Logging safety: diagnostic logger is fail-safe/no-throw; sanitizer redacts USB bus paths, local source/user paths, key/value secrets, colon/JSON-style secrets, bearer tokens, serial-like fields, and long token-like values.
- Capture behavior preserved: OTG capture failures still return explicit OTG failure and do not silently return native camera frames when USB is active.
- Packaging fix: binding project now excludes stale legacy `libnative-release.aar` / `libuvc-release.aar` when compatible `3.2.9` AARs exist, preventing duplicate Java class `com.jiangdg.natives.BuildConfig`.
- Validation after diagnostic-handler fixes:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android -v:minimal` — passed with generated-binding warnings and 0 errors.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed with Android 16 native page-size warnings and 0 errors.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed; APKs produced under `src/Vision2Audio.App/bin/Release/net10.0-android/android-arm64/publish/`.
- Remaining validation: rerun Android 11 physical OTG/UVC checks with the latest APK. If an exception recurs, collect the `[Diagnostics]` lines from Visual Studio output because they should now include the sanitized operation, exception type, message, and stack trace.

## Follow-up OTG capture unavailable fix

- Human report: after diagnostics were added, Android 11 validation showed `Prévia OTG/AUSBC indisponível para captura.` during capture.
- Root cause confirmed in code: `UsbCameraService.CaptureFrameAsync` still attempted to read `_previewView.Bitmap`; at capture time the preview `TextureView` could be unavailable even while the AUSBC session existed.
- Binding inspection confirmed generated APIs for `ICaptureCallBack.OnBegin()`, `ICaptureCallBack.OnComplete(string? path)`, `ICaptureCallBack.OnError(string? error)`, and inherited `CameraUVC.CaptureImage(ICaptureCallBack, string? savePath)`.
- Fix applied: OTG still capture now uses AUSBC `CaptureImage(...)` to write a JPEG under app-private cache `otg-captures`, reads the completed file bytes, deletes the temporary file best-effort, applies a 5-second timeout, and logs sanitized diagnostics on failure.
- Capture behavior preserved: OTG capture failure still returns explicit OTG failure and does not silently return a native camera frame.
- Validation after fix:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android -v:minimal` — passed with generated-binding warnings and 0 errors.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed with 0 warnings and 0 errors.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed; APKs produced under `src/Vision2Audio.App/bin/Release/net10.0-android/android-arm64/publish/`.
- Remaining validation: rerun Android 11 OTG capture. If failure remains, collect `[Diagnostics] Operation=otg-capture-ausbc-image` or `[Diagnostics] Operation=otg-capture-timeout` lines.

## Follow-up local secrets and capture-timeout diagnostics

- Human report: the app stopped reading `secrets.local`; human also added `AndroidUSBCamera-3.6.0.zip` and noted the maintained fork/successor should be preferred for new 2026 projects.
- Secret-loading fix: `AppPackageOpenAiSecretsProvider` now checks both `secrets.local.json` and `secrets.local`. The Android app project explicitly excludes both from default packaging, then includes them as app package assets only for Debug or when `IncludeLocalSecretsInAppPackage=true` is explicitly set.
- Secret validation: Release APK inspection found no entries matching `secrets.local`, `secrets.local.json`, `OPENAI`, `openAiApiKey`, or `apiKey`.
- AUSBC artifact status: no migration to `AndroidUSBCamera-3.6.0` has been recorded yet; the binding project still references the compatible `3.2.9` AAR set. The 3.6.0 zip remains a candidate for follow-up artifact evaluation if timeout persists.
- Capture-timeout follow-up: added sanitized capture diagnostics such as `otg-capture-ausbc-start` / callback-related diagnostics and changed AUSBC request render mode to OpenGL. Manual retest is required to confirm whether `CameraUVC.CaptureImage` now invokes callbacks instead of timing out.
- Validation after this follow-up:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android -v:minimal` — passed with generated-binding warnings.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed with Android 16 native page-size warnings.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed.
- Remaining validation: rerun Android 11 OTG preview/capture. If capture still times out, collect lines containing `otg-capture-ausbc-start`, `otg-capture-callback`, `OnBegin`, `OnComplete`, `OnError`, `otg-capture-timeout`, and `otg-capture-ausbc-image`.

## Follow-up priority capture UX cleanup

- Human priority change: OTG remains unresolved, but immediate priorities became physical Volume Up capture, recovery after capture, captured-image freeze, scrollable screen, and reduced visible UI text.
- Fix applied: Android `MainActivity` now treats `VolumeUp` as a capture trigger alongside existing keyboard/remote keys, ignores repeat events, and applies a short debounce to avoid duplicate captures.
- Capture flow fix: trigger path now routes through command state, uses a capture guard to prevent concurrent captures, and resets busy/command state so another capture can be taken after completion.
- Captured still behavior: after successful capture, the last image is shown over/in place of live preview and live preview is paused. Starting another capture clears the still and resumes the capture flow.
- UI cleanup: main content is inside a scroll view; visible main title `Vision 2 Audio`, `Prévia da câmera`, `Fonte ativa: ...`, and `Status da câmera: ...` labels were removed/reworded for a cleaner screen.
- Validation:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed with existing AUSBC/native Android 16 page-size warnings.
  - Debug Android build was blocked in one run by `Visual Studio 2026 Remote Debugger` locking `Vision2Audio.App.dll`; Release publish passed.
- Remaining validation: manual Android retest for scroll, removed text, Volume Up/remote capture, captured-image freeze, and second capture after freeze.

## Follow-up app polish, loading, audio lifecycle, and accessible prompt

- Human priorities: use `logo.jpeg` as app icon, show loading while the AI request is running, resume live camera after audio finishes, interrupt audio when the physical trigger is pressed during playback, always include exact/approximate location in the description, and always frame the description for a visually impaired user with movement guidance.
- App icon: `logo.jpeg` from the repository root is configured as the MAUI app icon. Release APK inspection confirmed generated `appicon`/`appicon_round` resources.
- Loading: main UI now shows an `ActivityIndicator` with `Analisando com IA...` specifically while the OpenAI/AI description request is active.
- Audio/preview lifecycle: after successful capture, the captured still remains while audio plays; after TTS finishes, the still clears and live preview resumes. If the trigger is pressed during audio playback, TTS is cancelled/interrupted, the still clears, preview resumes, and the app returns to ready state for the next photo. Concurrent capture guard remains.
- Prompt/location: OpenAI prompt always states that the user is visually impaired and asks for practical movement/navigation guidance, safety notes, obstacles, landmarks, and next steps in Brazilian Portuguese. Location context is always included: exact/approximate coordinates and accuracy when available, or an explicit unavailable/approximate note when GPS is unavailable.
- Location failure behavior: the coordinator no longer fails the whole request solely because GPS is unavailable; it proceeds with null/unavailable location context.
- Validation:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed with existing AUSBC native Android 16 page-size warnings.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed with existing warnings.
- Remaining validation: manual Android test for icon, AI loading indicator, audio-finish preview resume, trigger-during-audio interruption, repeated capture, and location/navigation guidance in returned descriptions.

## Follow-up human-readable location correction

- Human correction: location must not be described as latitude/longitude because a person cannot decode raw coordinates; it must be an address/place.
- Fix applied: GPS coordinates are still acquired internally, but `LocationService` now reverse-geocodes them through MAUI geocoding and formats a human-readable approximate address from available placemark fields such as street/number, neighborhood/locality, state/region, and country.
- Prompt behavior: OpenAI prompt now uses `Endereço/local aproximado da pessoa: ...` and explicitly instructs the model not to use or mention latitude/longitude to the user.
- Fallback behavior: if reverse geocoding fails, the prompt says the approximate address was not found; if GPS is unavailable, the request still proceeds and says location/address is unavailable. Raw coordinates are not exposed to the user-facing prompt.
- Validation:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed with existing AUSBC Android 16 page-size warnings.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed with existing warnings.
- Remaining validation: manual Android test with location permission enabled and disabled/network geocoding unavailable to confirm descriptions reference readable address/place or a no-address fallback, never latitude/longitude.

## Follow-up AI location translation wording

- Human request: ask the AI to translate longitude/latitude together with the image description.
- Implemented interpretation: the prompt now explicitly asks the AI to convert available location context into natural address/place language and integrate it with the scene description, while preserving the accessibility rule that raw latitude/longitude must not be spoken to the user.
- If reverse-geocoded address/place is available, the AI is instructed to use it naturally. If address lookup failed, the prompt says the address could not be identified and still forbids mentioning latitude/longitude numbers.
- Validation:
  - `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj -v:minimal` — passed 14/14.
  - `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -v:minimal` — passed after retry; existing AUSBC Android 16 page-size warnings remain.
  - `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -v:minimal` — passed with existing warnings.

## Follow-up resolution blocker fix

- Human report: the physical OTG/UVC camera resolution is 640x480 and Android 11 validation hit a resolution-related error.
- Root cause found in the Android AUSBC seam: the request used 640x480, but the preview view was a plain `TextureView` and the surface buffer/aspect-ratio state were not explicitly aligned to the same UVC size before `CameraUVC.OpenCamera(...)`.
- Fix applied: `CameraPreviewViewHandler` uses AUSBC `AspectRatioTextureView`; `UsbCameraService` prefers/safely selects 640x480 through AUSBC helpers when available, applies the selected size to the view and `SurfaceTexture`, and builds the `CameraRequest` with the same dimensions.
- Validation after fix: Android app Debug build succeeded. Android 11 physical validation remains blocked/pending and must be rerun.

## Follow-up USBMonitor class-resolution blocker

- Human report: selecting OTG fell back with `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;`.
- Root cause found in artifact/package inspection: `libausbc-release.aar` references `com.jiangdg.usb.USBMonitor`, but the older `libuvc-release.aar` defined `com.serenegiant.usb.USBMonitor` instead.
- Fix applied after `AndroidUSBCamera-3.3.3.zip` was added: extracted compatible `libuvc-3.2.9.aar`, `libnative-3.2.9.aar`, `libutils-3.2.9.aar`, and `libuvccommon-3.2.9.aar` to `external/ausbc/`; updated `Vision2Audio.AusbcBinding.csproj` to prefer the compatible AARs and include dependency-only utility/common AARs with `Bind=false`.
- Validation after fix: binding build passed; core tests passed 11/11; clean Release APK publish passed; APK DEX inspection found `Lcom/jiangdg/usb/USBMonitor;` in `classes2.dex`; APK secret asset inspection found no `secrets.local.json` or secret-like asset names.
- Remaining validation: rerun Android 11 OTG attach/permission/preview/capture/detach/reattach checks on physical hardware.

## Pending manual validation checklist

Use the latest clean Release APK after the AndroidUSBCamera 3.2.9 dependency update and diagnostic-handler fixes.

### Android 11 physical OTG/UVC device

- [ ] Install the latest Release APK on the Android 11 Xiaomi target device.
- [ ] Attach the 640x480 OTG/UVC camera through the intended OTG adapter.
- [ ] Launch the app and select `OTG` as the camera source.
- [ ] Grant Android USB permission when prompted.
- [ ] Confirm the previous `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;` error does not appear.
- [ ] If Visual Studio shows another `InvalidOperationException`, copy the nearby `[Diagnostics]` lines with operation, exception type, sanitized message, and stack trace.
- [ ] Confirm OTG preview starts and remains visible without falling back to front/rear camera.
- [ ] Capture an image and confirm the captured frame comes from the OTG preview source.
- [ ] Confirm the previous capture error `Prévia OTG/AUSBC indisponível para captura.` does not appear.
- [ ] Detach the camera and confirm the user sees explicit fallback/unavailable status.
- [ ] Reattach the camera, grant permission if prompted, select/retry OTG, and confirm preview can recover.
- [ ] Switch to front and rear native camera selections and confirm native preview/capture still work.
- [ ] Record pass/fail evidence: device model, Android version, APK path/build date, selected camera, observed status text, and any sanitized error message/logcat excerpt.

### Emulator/no-OTG environment

- [ ] Install the latest APK in an emulator or no-OTG device environment.
- [ ] Select `OTG` and confirm fallback remains explicit and usable.
- [ ] Confirm front/rear native preview/capture still work where emulator camera support is available.
- [ ] Record pass/fail evidence and any sanitized status/error text.

## Guardrails

- Do not mark acceptance passed without evidence.
- Do not hide deferred hardware validation.
