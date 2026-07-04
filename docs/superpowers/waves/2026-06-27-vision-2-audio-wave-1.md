# Wave Plan: Android Scene-Capture MVP

## Metadata

- Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `superseded`
- Tech Lead: `Tech Lead`
- Created: `2026-06-27`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec status: `approved`
- Spec version/date: `2026-06-27`
- Scope source: `Scope, acceptance criteria, business rules, constraints, non-functional requirements`

## Supersession note

This original MVP wave is retained as historical planning context. Later approved camera-preview and camera-selection specs changed the camera-source rules and introduced a dedicated AUSBC/USB Host hardening path. Current execution planning for real OTG/UVC support is moved to `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`.

## Planning preconditions

Planning may begin only when all are checked:

- [x] Spec is approved.
- [x] Stack is defined in `context/stack.md` or explicitly confirmed for this wave.
- [x] Required active agents are identified.
- [x] Required recruited agents are defined or explicitly not needed.
- [x] Required skills are defined or explicitly not needed.
- [x] Open questions do not block this wave.

## Wave objective

Deliver the client-only Android .NET MAUI MVP that can capture a scene on demand, send the image and GPS context directly to OpenAI, present the Brazilian Portuguese response in text and speech, and keep local read-only history with clear-all.

## In scope for this wave

Each item must trace back to the approved spec.

| Item | Spec reference | Reason |
| --- | --- | --- |
| Android .NET MAUI app shell and shared service wiring | Scope 42-53; Constraints 97-105 | Establish the client foundation required by the approved stack. |
| Local read-only history storage and clear-all behavior | Scope 52-53; Acceptance 82-88; Business rules 92-95 | Required product behavior for reviewing past captures. |
| Triggered scene capture from OTG camera plus GPS context and direct OpenAI request | Scope 43-46; Acceptance 66-76 | Core user action and primary value path. |
| Brazilian Portuguese response display, automatic speech playback, and offline warning | Scope 48-51; Acceptance 70-80; NFR 127-129 | Required output and failure behavior. |
| Validation evidence, acceptance review, and context updates | Context updates 133-143; Planning readiness 144-153 | Required durability and readiness tracking for the approved work. |

## Out of scope for this wave

| Item | Reason |
| --- | --- |
| Backend APIs or server-side processing | Explicitly out of scope in the spec. |
| User accounts or authentication flows | Explicitly out of scope in the spec. |
| Social sharing or history editing | Explicitly out of scope in the spec. |
| Offline scene description generation | Explicitly out of scope in the spec. |
| Non-Android platforms | Explicitly out of scope in the spec. |
| Project-management integration | Platform is still pending decision, so defer. |
| Backend or API specialist work | No backend exists and no internal API layer is approved. |

## Required agents

### Active agents

- `orchestrator.md`: execution coordination for the planned tasks.
- `maui-android-stack-specialist.md`: .NET MAUI / Android conventions and validation.
- `ux-specialist.md`: accessible UI, trigger flow, speech/text UX.
- `local-history-data-specialist.md`: local history storage and clear-all.
- `testing-specialist.md`: device and regression validation.
- `cybersecurity-specialist.md`: direct OpenAI secret handling, permissions, logging risk.
- `acceptance-specialist.md`: criteria traceability and release readiness.
- `docs-maintainer.md` or `context-maintainer.md`: context and durable documentation updates.

### Recruited agents

- Project-specific `vision2audio-*` recruited-agent names in this historical wave are superseded. Use the available agents listed above for future execution.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `csharp-developer` | `stack-specialist`, `frontend-specialist` | MAUI app structure, DI, Android integration, async flow. |
| `modern-csharp` | `stack-specialist`, `frontend-specialist` | Keep implementation idiomatic and maintainable. |
| `dotnet-csharp-dependency-injection` | `stack-specialist` | Wire services for capture, request, history, and TTS. |
| `dotnet-csharp-configuration` | `stack-specialist` | Handle approved app settings and secret-bound configuration. |
| `csharp-async-patterns` | `stack-specialist`, `frontend-specialist` | Network, GPS, camera, and speech flow coordination. |
| `csharp-developer` | `data-specialist` | Local read-only history storage and clear-all behavior. |
| `cybersecurity` | `cybersecurity-specialist` | Secret handling, permissions, logging, and external API exposure. |
| `verification-before-completion` | `testing-specialist`, `acceptance-specialist` | Evidence-first validation before completion claims. |

For future execution, map `stack-specialist` to `maui-android-stack-specialist`, `frontend-specialist` to `ux-specialist` when UX-only or `maui-android-stack-specialist` when MAUI implementation is involved, and `data-specialist` to `local-history-data-specialist`.

## Tasks

| Task ID | Title | Size | Category | Depends on | Spec reference |
| --- | --- | --- | --- | --- | --- |
| `vision2audio-1.1` | MAUI Android app shell and shared contracts | `M` | `standard` | `none` | `Scope 42-53; Constraints 97-105` |
| `vision2audio-1.2` | Local read-only history storage and clear-all | `S` | `standard` | `vision2audio-1.1` | `Scope 52-53; Acceptance 82-88; Business rules 92-95` |
| `vision2audio-1.3` | Triggered capture, GPS context, and direct OpenAI request | `L` | `standard` | `vision2audio-1.1, vision2audio-1.2` | `Scope 43-46; Acceptance 66-76` |
| `vision2audio-1.4` | Portuguese response UI, speech playback, and offline warning | `M` | `standard` | `vision2audio-1.3` | `Scope 48-51; Acceptance 70-80; NFR 127-129` |
| `vision2audio-1.5` | Validation, acceptance evidence, and context updates | `M` | `standard` | `vision2audio-1.4` | `Context updates 133-143; Planning readiness 144-153` |

## Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| OTG camera, Bluetooth, and GPS behavior varies by Android device | Capture path may fail on some hardware | Validate on target hardware early and document device assumptions. |
| Direct OpenAI usage can expose secrets if mishandled | Security or release blocker | Keep secrets out of logs, use approved config only, and review permissions. |
| Offline detection may be unreliable without device testing | User may see stale or misleading warnings | Validate network state checks on device and keep the warning explicit. |
| Brazilian Portuguese output quality may drift | Core UX may not meet accessibility expectations | Verify prompt/output handling and speak/display the same approved text. |

## Quality gate plan

Every task must report:

- `review`
- `tests`
- `acceptance`
- `security`

## Human review points

- Review the end-to-end capture flow before the wave is closed.

## Completion criteria

- [ ] Historical task statuses reconciled with implementation evidence.
- [ ] Real OTG/UVC camera hardening tracked in Wave 2.
- [ ] Hardware validation evidence recorded or explicitly deferred.

## Guardrails

- Do not include work that cannot be traced to the approved spec.
- Do not use this wave to expand product scope.
- Do not plan implementation before stack and required agents are defined.
- If scope changes are needed, return to Product Owner and Spec Writer.
