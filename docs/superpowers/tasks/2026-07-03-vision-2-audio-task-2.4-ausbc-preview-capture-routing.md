# Task: Route OTG preview and still capture through the same AUSBC session

## Metadata

- Task ID: `vision2audio-2.4`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `completed`
- Size: `M`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Acceptance 70-72; Business rules 80-82`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec reference: `Acceptance 71-77; Business rules 81-84`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.4`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Route OTG preview and still-frame capture through the active AUSBC session so the frame sent to OpenAI comes from the same source shown in the preview.

## Expected result

When OTG is selected and available, the preview uses AUSBC and capture obtains a still frame from that same OTG session; native front/rear fallback remains unchanged.

## In scope

- Route `CameraPreviewViewHandler` OTG mode through AUSBC — source: `preview spec`.
- Route `UsbCameraCaptureService` through the same active AUSBC session — source: `preview/capture synchronization acceptance`.
- Preserve Camera2/native path for front/rear fallback — source: `selection spec`.
- Add validation for selection change updating preview and capture together — source: `selection spec Acceptance 71-77`.

## Out of scope

- Video recording.
- Gallery or sharing.
- Replacing native Camera2 fallback.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `ausbc-android-binding-specialist`: AUSBC preview/capture routing.
- `maui-android-stack-specialist`: handler/service integration and source coordinator boundary.
- `testing-specialist`: synchronization and build validation.
- `acceptance-specialist`: criteria review.

### Recruited agents

- `ausbc-android-binding-specialist`.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `dotnet-android-ausbc-binding` | `ausbc-android-binding-specialist` | AUSBC preview/capture APIs. |
| `csharp-developer` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | MAUI Android service and handler changes. |
| `csharp-async-patterns` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Preview/capture lifecycle and cancellation. |
| `verification-before-completion` | `testing-specialist`, `acceptance-specialist` | Evidence-first completion. |

## Files or areas expected to change

- `src/Vision2Audio.App/Platforms/Android/CameraPreviewViewHandler.cs`
- `src/Vision2Audio.App/Platforms/Android/UsbCameraService.cs`
- `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs`
- `src/Vision2Audio.App/Services/IUsbCameraService.cs`
- `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` if coordinator behavior needs updated coverage

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Test command: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- Manual validation: select OTG on Android 11 with UVC camera, verify preview starts, trigger capture, and verify captured frame comes from OTG.
- Manual validation: switch to front/rear and confirm preview and capture both change to the selected native source.
- Security validation: verify no image payload or sensitive device identifiers are logged.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `passed` | MAUI Android review confirmed OTG preview routes through AUSBC, USB capture no longer silently returns native frames, AUSBC cleanup runs on lifecycle stop paths, native fallback is preserved, and vendor types remain Android-only. |
| tests | `passed` | Android Debug build passed with 9 existing generated AUSBC warnings; core tests passed 10/10; Debug APK contains no `secrets.local.json`. |
| acceptance | `passed` | Preview/capture source synchronization is enforced in code and regression tests; real Android 11 hardware proof remains deferred to Wave 2 validation. |
| security | `passed` | Security review confirmed AUSBC sessions close on detach/dispose/surface destruction and no image payloads, API keys, GPS payloads, USB serials, full `UsbDevice.ToString()`, or local secrets were exposed. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Still-frame extraction is not available from AUSBC API | AUSBC specialist | Document blocker and identify supported frame callback/snapshot API. |
| Source switching leaves stale preview session | MAUI Android stack specialist | Ensure old session is stopped before new source starts. |

## Completion report

- What changed: Routed OTG preview through the AUSBC `IUsbCameraService` session, routed OTG capture through the same AUSBC-backed preview/session, removed the OTG Camera2 success path, prevented silent native capture fallback when active USB capture fails, closed AUSBC sessions on handler stop/dispose/detach/surface destruction, stopped previous preview sources during source changes, updated core tests, and updated `docs/ausbc-binding.md`.
- Agents used: `ausbc-android-binding-specialist`, `maui-android-stack-specialist`, `testing-specialist`, `cybersecurity-specialist`.
- Skills used: `dotnet-android-ausbc-binding`, `csharp-developer`, `csharp-async-patterns`, `verification-before-completion`, `cybersecurity`.
- Validation executed: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android` passed; `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` passed 10/10; Debug APK asset inspection found no `secrets.local.json`.
- Validation not executed and why: Android 11 physical OTG/UVC validation, runtime logcat review, and release packaging were not executed; they are deferred to Wave 2 validation and release-readiness work.
- Context updates completed or needed: No context update required for this task beyond existing Wave 2 context.
- Documentation updates completed or needed: `docs/ausbc-binding.md` updated with routing and lifecycle notes.
- Remaining risks: Real hardware behavior for `CameraUVC.OpenCamera(TextureView, CameraRequest)`, still capture from the preview bitmap, detach/reattach, and fallback recovery remains unproven until Android 11 device validation.
- Recommended next step: Execute `vision2audio-2.5` to preserve fallback/status UX and security constraints, then `vision2audio-2.6` for Android 11 hardware and acceptance evidence.

## Guardrails

- Do not open separate unsynchronized preview and capture sessions.
- Do not remove native fallback behavior.
