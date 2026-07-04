# Task: Real OTG/UVC camera support through AUSBC binding

## Metadata

- Task ID: `vision2audio-ausbc-otg-binding`
- Related Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design; 2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `superseded`
- Size: `L`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Scope 42-46; Acceptance 66-76; Constraints 97-105`
- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Scope 41-46; Acceptance 58-76; Business rules 78-82`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec reference: `Scope 42-48; Acceptance 59-77; Business rules 79-84`
- Wave file: `docs/superpowers/waves/2026-06-27-vision-2-audio-wave-1.md`
- Wave task row/reference: `vision2audio-1.3` capture source, `vision2audio-1.4` user-visible status, and human-approved continuation after Android Camera2 did not expose the OTG camera.

## Supersession note

This large AUSBC task has been split into Wave 2 executable tasks:

- `vision2audio-2.1` — reconcile camera-source rule and binding readiness.
- `vision2audio-2.2` — inspect and stabilize AUSBC binding API surface.
- `vision2audio-2.3` — implement Android USB Host/AUSBC session boundary.
- `vision2audio-2.4` — route OTG preview and still capture through the same AUSBC session.
- `vision2audio-2.5` — preserve fallback/status UX and security constraints.
- `vision2audio-2.6` — validate Android 11 hardware, emulator fallback, and acceptance evidence.

Use `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md` for execution planning.

## Execution preconditions

Execution may begin only when all are checked:

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan or explicitly approved as an ad hoc task by the human.
- [x] Stack context is defined or not applicable.
- [x] Required agents are defined.
- [x] Required skills are defined or not applicable.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Connect the existing AUSBC .AAR binding work to real Android OTG/UVC preview and still-frame capture so the selected OTG camera path uses USB Host/AUSBC instead of relying on Camera2 external-camera exposure.

## Expected result

When the user selects OTG on supported Android hardware, the app requests USB camera permission, starts a real UVC preview through AUSBC, captures a frame from the same OTG source for the OpenAI flow, and preserves native/front/rear fallback behavior when OTG is unavailable or fails.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- Inspect generated AUSBC binding API surface and adjust minimal `Metadata.xml` transforms if needed — source: approved OTG camera scope and implementation blocker.
- Implement Android-only AUSBC adapter in `UsbCameraService` or a narrow adjacent service — source: OTG capture/preview requirements.
- Route `CameraPreviewViewHandler` OTG preview through AUSBC while preserving Camera2 for native/front/rear — source: preview and camera-selection specs.
- Route OTG still capture through the same active AUSBC preview/session — source: preview/capture synchronization acceptance criteria.
- Preserve alerts, fallback messages, emulator usability, and native camera fallback — source: camera preview and selection specs.
- Document validation results and any hardware limitations — source: wave validation requirements.

## Out of scope

- New product behavior outside camera preview/capture.
- Backend, authentication, sharing, gallery, video recording, or history editing.
- Replacing the native/front/rear Camera2 fallback.
- Publishing to app stores.
- Claiming full Android 16 compatibility for bundled native libraries without vendor-fixed 16 KB page-size `.so` artifacts.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `ausbc-android-binding-specialist`: primary implementation/review for .NET Android AAR binding, AUSBC/UVC interop, USB Host lifecycle, and native library packaging.
- `maui-android-stack-specialist`: MAUI handler/service integration, DI boundary, Android permissions, and fallback preservation.
- `testing-specialist`: build/test validation and device validation checklist.
- `cybersecurity-specialist`: permission/logging/secret exposure review.
- `acceptance-specialist`: acceptance criteria and user-visible fallback/status review.
- `docs-maintainer` or `context-maintainer`: update durable current-state/decision documentation after execution.

### Recruited agents

- `ausbc-android-binding-specialist`: `.opencode/agent-blueprints/stack-specialist.md` / required for AUSBC .AAR binding and UVC interop.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `dotnet-android-ausbc-binding` | `ausbc-android-binding-specialist` | Required workflow for AUSBC .AAR binding, metadata transforms, USB Host, native `.so` packaging, and MAUI boundary integration. |
| `csharp-developer` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | C#/.NET MAUI Android implementation. |
| `modern-csharp` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Keep C# implementation idiomatic and maintainable. |
| `csharp-async-patterns` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Camera permission/session/capture flow must remain responsive and cancellable. |
| `dotnet-csharp-dependency-injection` | `maui-android-stack-specialist` | Preserve service registration boundaries. |
| `systematic-debugging` | `ausbc-android-binding-specialist`, `testing-specialist` | Diagnose binding/runtime/hardware failures before changing code. |
| `verification-before-completion` | `testing-specialist`, `acceptance-specialist` | Evidence-first completion. |
| `cybersecurity` | `cybersecurity-specialist` | Review Android permissions, logs, and direct-to-OpenAI secret handling. |

## Files or areas expected to change

- `src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj`
- `src/Vision2Audio.AusbcBinding/Transforms/Metadata.xml`
- `src/Vision2Audio.AusbcBinding/obj/**/api.xml` and generated binding output for inspection only.
- `src/Vision2Audio.App/Platforms/Android/UsbCameraService.cs`
- `src/Vision2Audio.App/Platforms/Android/CameraPreviewViewHandler.cs`
- `src/Vision2Audio.App/Services/IUsbCameraService.cs`
- `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs`
- `src/Vision2Audio.App/MauiProgram.cs` if a narrow DI change is required.
- `docs/ausbc-binding.md` and `.opencode/context/current-state.md` after validation.

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android`
- Build/check command: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Test command: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- Package command: `dotnet publish src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
- Manual validation: on Android 11 Xiaomi device with OTG/UVC camera attached, grant USB permission, select OTG, verify preview starts, capture uses OTG frame, detach shows fallback/alert, reattach can recover, native/front/rear still work.
- Manual validation: emulator without OTG remains usable and clearly falls back.
- Security validation: confirm no OpenAI key, USB device serial, full environment dump, image payload, or sensitive path is logged.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending` | Binding/API/lifecycle changes must be reviewed by AUSBC and MAUI Android specialists. |
| tests | `pending` | Build, unit tests, APK publish, and device/emulator validation required. |
| acceptance | `pending` | Must satisfy OTG preview/capture and fallback criteria or document explicit deferral. |
| security | `pending` | Android permissions and logging/secret handling review required. |

## Execution notes

- Current build status before this task: app and AUSBC binding compile with 5 Android 16 page-size warnings from bundled native `.so` files and 0 errors.
- Android Camera2 did not expose the target USB camera on the Xiaomi Android 11 device; AUSBC/USB Host is the selected integration path.
- Keep AUSBC Java/Kotlin objects isolated to Android platform code; shared code should continue using interfaces and DTOs.
- Do not edit generated `api.xml`; inspect it and use `Transforms/Metadata.xml` only for durable binding fixes.
- Do not remove the existing diagnostic alerts because the user cannot use OTG and USB debugging at the same time.

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| New agent/skill were created during the current opencode session and may not be loaded until restart. | Human / Tech Lead | Restart opencode before delegating to `ausbc-android-binding-specialist`. |
| AUSBC generated API may not expose the needed preview/capture methods cleanly. | AUSBC Android Binding Specialist | Inspect `api.xml` and generated C#; add minimal transforms or escalate. |
| Native `.so` libraries warn about Android 16 16 KB page-size support. | AUSBC Android Binding Specialist | Accept for Android 11 validation or source updated vendor libraries before Android 16 readiness claims. |
| Real UVC behavior cannot be fully proven without physical OTG hardware. | Testing / Acceptance Specialist | Run Android 11 device validation or document hardware validation pending. |
| Runtime USB permission/lifecycle failures may occur without debug USB attached. | MAUI Android Stack Specialist | Surface user-facing alerts and capture logcat where possible. |

## Completion report

- What changed:
- Agents used:
- Skills used:
- Validation executed:
- Validation not executed and why:
- Context updates completed or needed:
- Documentation updates completed or needed:
- Remaining risks:
- Recommended next step:

## Guardrails

- Do not execute work that is not traceable to an approved spec, approved wave, or explicit human-approved ad hoc request.
- Do not expand scope during execution.
- If the task requires scope expansion, return to Tech Lead or Product Owner.
- Do not finalize the task with failed or missing gates.
