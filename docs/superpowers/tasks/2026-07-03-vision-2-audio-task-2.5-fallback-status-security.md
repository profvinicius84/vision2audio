# Task: Preserve fallback/status UX and security constraints

## Metadata

- Task ID: `vision2audio-2.5`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-27-vision-2-audio-design; 2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `completed`
- Size: `S`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `NFR 128-129`
- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Acceptance 66-76`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec reference: `Acceptance 67-77`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.5`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Ensure user-visible fallback/status behavior remains clear and accessible after AUSBC routing, and verify security constraints around permissions, logs, and direct OpenAI configuration.

## Expected result

The app clearly reports selected source, active source, fallback, unavailable camera status, and OTG failure states without exposing sensitive data.

## In scope

- Preserve visible fallback messages for unavailable OTG/emulator cases — source: `preview and selection specs`.
- Add or verify “no camera source available” status — source: `preview spec Acceptance 74-76`.
- Verify OTG selected on emulator falls back visibly — source: `selection spec Acceptance 75-77`.
- Review Android permissions and logs — source: `MVP NFR 128-129`.

## Out of scope

- New visual design beyond required status clarity.
- New analytics or telemetry.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `ux-specialist`: accessible copy and status clarity.
- `maui-android-stack-specialist`: UI binding and source status integration.
- `cybersecurity-specialist`: permissions/logging/secret review.
- `acceptance-specialist`: acceptance coverage.

### Recruited agents

- None.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `csharp-developer` | `maui-android-stack-specialist` | MAUI UI/status integration. |
| `csharp-async-patterns` | `maui-android-stack-specialist` | Source status lifecycle. |
| `cybersecurity` | `cybersecurity-specialist` | Permissions, logs, and secret-handling review. |
| `verification-before-completion` | `acceptance-specialist` | Evidence-first acceptance. |

## Files or areas expected to change

- `src/Vision2Audio.App/MainPage.xaml`
- `src/Vision2Audio.App/ViewModels/MainViewModel.cs`
- `src/Vision2Audio.App/Platforms/Android/UsbCameraService.cs`
- `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml` if permission declarations need adjustment

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android`
- Test command: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj`
- Manual validation: emulator without OTG remains usable and clearly reports fallback.
- Manual validation: no-camera-source path shows a clear unavailable status if both OTG and native fail.
- Security validation: confirm no OpenAI key, USB serial, full environment dump, image payload, or sensitive path is logged.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `passed` | UX review confirmed Brazilian Portuguese status clarity for fallback active, selected camera unavailable, and no camera source available without out-of-scope redesign. |
| tests | `passed` | Android build passed with 9 existing generated AUSBC warnings; core tests passed 11/11. |
| acceptance | `passed` | OTG unavailable fallback, no-camera-source status, and visible fallback label/state are covered by code and tests; manual emulator/hardware evidence remains deferred to task 2.6. |
| security | `passed` | Security review confirmed aggregate USB diagnostics, backup disabled, default secret packaging excluded, no sensitive payload logging, and preview restart errors sanitized. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Diagnostic alerts expose sensitive device data | Cybersecurity specialist | Redact logs/status text. |
| Fallback state is confusing to visually impaired users | UX specialist | Use direct Brazilian Portuguese status text and accessible announcements. |

## Completion report

- What changed: Brazilian Portuguese camera-source status now says when fallback is active and when no camera source is available; fallback now carries the active selection so the preview uses the fallback source instead of retrying unavailable OTG; preview panel labels show active source/status explicitly; OTG unavailable diagnostics shown to the user are aggregated and do not include USB names, serials, vendor/product identifiers or full device strings; debug logs no longer print full unhandled/startup exceptions; Android backup is disabled to reduce local secret/history backup exposure.
- Agents used: `maui-android-stack-specialist`, `ux-specialist`, `testing-specialist`, `cybersecurity-specialist`.
- Skills used: `csharp-developer`, `csharp-async-patterns`, `writing-csharp-code`, `cybersecurity`, `executing-plans`, `verification-before-completion`.
- Validation executed: `dotnet test tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` passed (11/11); `dotnet build src/Vision2Audio.App/Vision2Audio.App.csproj -f net10.0-android` passed with existing generated AUSBC binding warnings; security review passed after sanitizing preview restart exception handling.
- Validation not executed and why: Manual emulator/hardware validation is deferred to `vision2audio-2.6`, which is scoped for Android 11 hardware and emulator evidence.
- Context updates completed or needed: No architecture/context update needed for this small UX/security hardening; Android 16 16 KB native-library warning remains tracked by the wave risk.
- Documentation updates completed or needed: This task report was updated.
- Remaining risks: AUSBC third-party/native libraries may still emit their own Android logs outside app code; Android 16 16 KB page-size warnings remain; real-device OTG behavior still requires task `vision2audio-2.6` evidence.
- Recommended next step: Review changes, then run `vision2audio-2.6` emulator and Android 11 hardware validation.

## Guardrails

- Do not add telemetry or analytics.
- Do not hide fallback failures silently.
