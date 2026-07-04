# Task: Implement Android USB Host/AUSBC session boundary

## Metadata

- Task ID: `vision2audio-2.3`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-27-vision-2-audio-design; 2026-06-29-camera-preview-panel-design`
- Status: `completed`
- Size: `M`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Acceptance 66-76; Constraints 101-104`
- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Scope 43-46`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.3`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Create a narrow Android-only AUSBC/USB Host session boundary that requests permission, opens/closes the UVC camera, and exposes lifecycle-safe preview/capture hooks to existing MAUI services.

## Expected result

Android platform code can establish and dispose an AUSBC session without leaking vendor objects into shared code.

## In scope

- Implement or adapt `UsbCameraService` / adjacent Android platform service for AUSBC session lifecycle — source: `MVP OTG camera scope`.
- Request and handle USB camera permission — source: `Android OTG constraint`.
- Keep AUSBC Java/Kotlin types isolated to Android platform code — source: `architecture guardrail`.
- Preserve existing diagnostic alerts needed when USB debugging is unavailable — source: `human-approved blocker handling`.

## Out of scope

- UI redesign.
- Direct OpenAI request changes.
- Native front/rear Camera2 replacement.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `ausbc-android-binding-specialist`: AUSBC lifecycle and binding interop.
- `maui-android-stack-specialist`: Android service/DI boundary and permissions.
- `cybersecurity-specialist`: permission/logging review.

### Recruited agents

- `ausbc-android-binding-specialist`.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `dotnet-android-ausbc-binding` | `ausbc-android-binding-specialist` | AUSBC session integration. |
| `csharp-developer` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | C# Android platform service implementation. |
| `csharp-async-patterns` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Permission/session cancellation and responsiveness. |
| `dotnet-csharp-dependency-injection` | `maui-android-stack-specialist` | Register narrow Android service boundaries. |
| `cybersecurity` | `cybersecurity-specialist` | Permission and log exposure review. |

## Files or areas expected to change

- `src/Vision2Audio.App/Platforms/Android/UsbCameraService.cs`
- `src/Vision2Audio.App/Services/IUsbCameraService.cs`
- `src/Vision2Audio.App/MauiProgram.cs` if DI changes are required
- `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml` if permission declarations need adjustment

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Test command: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- Manual validation: on Android 11 with OTG camera attached, permission request appears and session open/close status is visible.
- Security validation: confirm logs do not include OpenAI keys, USB serials, image payloads, or full environment dumps.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `passed` | MAUI Android stack review passed: receiver flags, permission hardening, session cleanup, vendor isolation, fallback preservation, and default secret exclusion verified. |
| tests | `passed` | Debug Android app build passed with 9 existing generated AUSBC warnings; core tests passed 9/9; Debug APK contained no `secrets.local.json`. |
| acceptance | `passed` | USB Host/AUSBC session boundary is ready for later preview/capture routing; real Android 11 hardware validation remains deferred to Wave 2 validation task. |
| security | `passed` | Cybersecurity review passed after sanitizing OpenAI upstream error bodies and verifying no default secret packaging, serials, full `UsbDevice.ToString()`, image payloads, or API keys were exposed. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Runtime USB permission behavior differs by device | MAUI Android stack specialist | Validate on target Android 11 hardware. |
| AUSBC session lifecycle crashes on detach | AUSBC specialist | Add explicit detach/error handling before routing capture. |

## Completion report

- What changed: Added Android-only AUSBC/USB Host session boundary in `UsbCameraService`, added `CloseSessionAsync` support through `IUsbCameraService`, hardened USB permission/detach receiver registration and permission recheck, closed sessions on fallback/stop paths, prevented default packaging of `secrets.local.json`, sanitized OpenAI upstream error body exposure, and updated `docs/ausbc-binding.md`.
- Agents used: `ausbc-android-binding-specialist`, `maui-android-stack-specialist`, `testing-specialist`, `cybersecurity-specialist`.
- Skills used: `dotnet-android-ausbc-binding`, `csharp-developer`, `csharp-async-patterns`, `dotnet-csharp-dependency-injection`, `systematic-debugging`, `verification-before-completion`, `cybersecurity`.
- Validation executed: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android` passed; `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` passed 9/9; Debug APK inspection found no `secrets.local.json` entry.
- Validation not executed and why: Android 11 OTG/UVC hardware validation, runtime logcat review, and Release packaging were not completed; they are deferred to later Wave 2 validation. Release AUSBC binding build currently has generated Xlog formatter errors and must be handled before release packaging readiness.
- Context updates completed or needed: No context update required for this task beyond existing Wave 2 context; Release binding issue should be tracked before task 2.6/package validation.
- Documentation updates completed or needed: `docs/ausbc-binding.md` updated with session-boundary and packaging/security notes.
- Remaining risks: Real device permission/open/close/detach behavior remains unvalidated; Release AUSBC binding build failure blocks later release/package readiness; explicit opt-in can still package local secrets and must remain disabled outside local development.
- Recommended next step: Execute `vision2audio-2.4` to route OTG preview and still capture through the same AUSBC session, after noting the Release binding follow-up for packaging validation.

## Guardrails

- Keep vendor objects inside Android platform code.
- Do not alter camera-selection product rules.
