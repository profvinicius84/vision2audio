# Wave Plan: Camera Source / AUSBC Hardening

## Metadata

- Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-27-vision-2-audio-design; 2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `blocked-on-manual-validation`
- Tech Lead: `Tech Lead`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec status: `approved`
- Spec version/date: `2026-06-27`
- Scope source: `Scope 42-53; Acceptance 66-88; Constraints 97-105`
- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec status: `approved`
- Spec version/date: `2026-06-29`
- Scope source: `Scope 41-46; Acceptance 58-76; Business rules 80-82`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec status: `approved`
- Spec version/date: `2026-07-01`
- Scope source: `Scope 42-48; Acceptance 59-77; Business rules 81-84`

## Planning preconditions

Planning may begin only when all are checked:

- [x] Spec is approved.
- [x] Stack is defined in `.opencode/context/stack.md`.
- [x] Required active agents are identified.
- [x] Required recruited agents are defined or explicitly not needed.
- [x] Required skills are defined or explicitly not needed.
- [x] Open questions do not block this wave.

## Wave objective

Harden the Android camera-source path so the user-selected camera source controls preview and capture, OTG/UVC uses AUSBC/USB Host on real hardware, fallback remains explicit and accessible, and Android 11 device validation is recorded.

## In scope for this wave

| Item | Spec reference | Reason |
| --- | --- | --- |
| User-selected camera source has precedence over the older OTG-first default | Camera selection spec Scope 42-48; Business rules 81-84 | The approved selection spec supersedes the earlier implicit OTG-first behavior. |
| Preview and capture stay synchronized with the active source | Preview spec Acceptance 70-72; Selection spec Acceptance 71-77 | Prevents the app from showing one source and capturing another. |
| Real OTG/UVC preview and still capture use AUSBC/USB Host when OTG is selected and available | MVP spec Scope 43-46; Preview spec Scope 43-45; Selection spec Scope 42-48 | Camera2 did not expose the target USB camera on Android 11. |
| Native front/rear fallback remains available and clearly indicated | Preview spec Acceptance 66-76; Selection spec Acceptance 67-77 | Required for emulator and unavailable-camera scenarios. |
| Android 11 hardware validation and evidence recording | MVP Risks 120-123; Preview NFR 109-112; Selection NFR 111-114 | OTG behavior cannot be accepted without device evidence or an explicit deferral. |

## Out of scope for this wave

| Item | Reason |
| --- | --- |
| Backend, authentication, sharing, gallery, video recording, history editing | Explicitly outside approved specs. |
| App store publishing | Not part of the approved specs. |
| Full Android 16 native-library readiness claims | Existing AUSBC bundled `.so` files warn about 16 KB page-size support; Android 11 validation is the target for this wave. |
| Refactoring unrelated GPS, OpenAI, TTS, or history behavior | Must not expand beyond camera-source hardening. |

## Required agents

### Active agents

- `orchestrator.md`: execution coordination for planned tasks.
- `ausbc-android-binding-specialist.md`: AUSBC .AAR binding, metadata transforms, UVC/USB Host lifecycle, native library packaging.
- `maui-android-stack-specialist.md`: MAUI Android handlers, services, DI boundaries, permissions, fallback preservation.
- `testing-specialist.md`: build, unit, package, emulator, and device validation evidence.
- `cybersecurity-specialist.md`: Android permissions, logs, local config, and direct OpenAI secret exposure review.
- `acceptance-specialist.md`: spec traceability and user-visible acceptance review.
- `docs-maintainer.md` or `context-maintainer.md`: durable state, decision, and validation documentation.

### Recruited agents

- `ausbc-android-binding-specialist`: available project-specific specialist for AUSBC/UVC OTG binding work.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `dotnet-android-ausbc-binding` | `ausbc-android-binding-specialist` | Required for AUSBC .AAR binding, metadata transforms, USB Host, native `.so` packaging, and MAUI boundary integration. |
| `csharp-developer` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | C#/.NET MAUI Android implementation. |
| `modern-csharp` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Keep C# implementation idiomatic and maintainable. |
| `csharp-async-patterns` | `ausbc-android-binding-specialist`, `maui-android-stack-specialist` | Permission, session, capture, and cancellation flow. |
| `dotnet-csharp-dependency-injection` | `maui-android-stack-specialist` | Preserve service registration boundaries. |
| `systematic-debugging` | `ausbc-android-binding-specialist`, `testing-specialist` | Diagnose binding/runtime/hardware failures before changing code. |
| `verification-before-completion` | `testing-specialist`, `acceptance-specialist` | Evidence-first completion. |
| `cybersecurity` | `cybersecurity-specialist` | Review Android permissions, logs, and direct-to-OpenAI secret handling. |

## Tasks

| Task ID | Title | Size | Category | Depends on | Spec reference |
| --- | --- | --- | --- | --- | --- |
| `vision2audio-2.1` | Reconcile camera-source rule and binding readiness | `S` | `standard` | `none` | Selection spec Business rules 81-84; Preview spec Business rules 80-82 |
| `vision2audio-2.2` | Inspect and stabilize AUSBC binding API surface | `M` | `standard` | `vision2audio-2.1` | MVP spec Scope 43-46; Constraints 101 |
| `vision2audio-2.3` | Implement Android USB Host/AUSBC session boundary | `M` | `standard` | `vision2audio-2.2` | MVP spec Acceptance 66-76; Preview spec Scope 43-46 |
| `vision2audio-2.4` | Route OTG preview and still capture through the same AUSBC session | `M` | `standard` | `vision2audio-2.3` | Preview spec Acceptance 70-72; Selection spec Acceptance 71-77 |
| `vision2audio-2.5` | Preserve fallback/status UX and security constraints | `S` | `standard` | `vision2audio-2.4` | Preview spec Acceptance 66-76; Selection spec Acceptance 67-77; MVP NFR 128-129 |
| `vision2audio-2.6` | Validate Android 11 hardware, emulator fallback, and acceptance evidence | `M` | `standard` | `vision2audio-2.5` | All three specs' acceptance criteria related to camera source, fallback, capture, and context updates |

## Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| AUSBC generated API does not expose preview/capture methods cleanly | Blocks real OTG frame capture | Inspect generated binding first and use minimal `Metadata.xml` transforms. |
| Native `.so` libraries emit Android 16 16 KB page-size warnings | Android 16 readiness cannot be claimed | Keep this wave scoped to Android 11 validation and document warning. |
| Real UVC behavior cannot be proven without hardware | Acceptance remains blocked | Require Android 11 device evidence or explicit human-approved deferral. |
| Preview/capture can drift if separate sessions are opened | User sees one source and captures another | Task 2.4 must route both through the same active session. |
| Direct OpenAI config/logging risk remains present | Security gate can fail | Security review must verify no key, payload, serial, or environment dump is logged. |

## Quality gate plan

Every task must report:

- `review`
- `tests`
- `acceptance`
- `security`

## Human review points

- Confirm that the camera-selection spec supersedes older OTG-first behavior.
- Review Android 11 OTG/UVC validation evidence before closing the wave.
- Decide whether any missing hardware validation is acceptable as an explicit deferral.

## Completion criteria

- [ ] All tasks completed or explicitly deferred with rationale. Current state: tasks 2.1-2.5 completed; task 2.6 remains blocked on manual device/emulator validation.
- [ ] Build, tests, APK publish, emulator fallback, and Android 11 hardware validation evidence recorded. Current state: build/test/package/DEX/secret checks and diagnostic-handler validation recorded; emulator fallback and Android 11 hardware validation remain pending.
- [ ] Security and acceptance gates passed or explicitly waived by the human.
- [ ] `.opencode/context/current-state.md`, `.opencode/context/architecture.md`, `.opencode/context/decisions.md`, `.opencode/context/stack.md`, and `docs/ausbc-binding.md` updated when execution changes the project state.

## Guardrails

- Do not include work that cannot be traced to the approved specs or the documented Camera2 blocker.
- Do not use this wave to add unrelated camera features.
- Do not claim full Android 16 support from current AUSBC native libraries.
- If a product behavior change is needed, return to Product Owner and Spec Writer.
