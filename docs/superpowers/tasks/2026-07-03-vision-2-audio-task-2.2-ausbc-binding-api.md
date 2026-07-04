# Task: Inspect and stabilize AUSBC binding API surface

## Metadata

- Task ID: `vision2audio-2.2`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `completed`
- Size: `M`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Scope 43-46; Constraints 101`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.2`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Inspect the generated AUSBC .NET Android binding and apply only minimal durable metadata transforms needed for usable preview/capture APIs.

## Expected result

The binding project builds, generated API names needed for preview/capture are known, and any required `Metadata.xml` transforms are documented and minimal.

## In scope

- Build `Vision2Audio.AusbcBinding` — source: `wave 2`.
- Inspect generated `api.xml` and generated C# output — source: `AUSBC blocker`.
- Update `Transforms/Metadata.xml` only if required — source: `binding readiness`.
- Document API names and limitations — source: `docs/ausbc-binding.md`.

## Out of scope

- Editing generated `api.xml` directly.
- Implementing app preview/capture routing.
- Replacing vendor native `.so` libraries unless binding cannot build.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `ausbc-android-binding-specialist`: binding/API surface owner.
- `testing-specialist`: build evidence.
- `cybersecurity-specialist`: dependency and logging risk review if metadata or packaging changes expose surfaces.

### Recruited agents

- `ausbc-android-binding-specialist`.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `dotnet-android-ausbc-binding` | `ausbc-android-binding-specialist` | Binding inspection and metadata transforms. |
| `csharp-developer` | `ausbc-android-binding-specialist` | .NET Android binding project work. |
| `systematic-debugging` | `ausbc-android-binding-specialist`, `testing-specialist` | Diagnose build or generated API failures. |
| `verification-before-completion` | `testing-specialist` | Evidence-first build completion. |

## Files or areas expected to change

- `src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj`
- `src/Vision2Audio.AusbcBinding/Transforms/Metadata.xml`
- `src/Vision2Audio.AusbcBinding/obj/**/api.xml` for inspection only
- `docs/ausbc-binding.md`

## Validation plan

- Build/check command: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android`
- Test command: `not-applicable; binding build task`
- Manual validation: record the generated API names needed by downstream preview/capture tasks.
- Security validation: confirm no secrets, full environment dumps, or device identifiers are documented/logged.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `passed` | AUSBC specialist inspected `api.xml` and generated C# output; usable downstream surface exists without `Metadata.xml` changes. |
| tests | `passed` | Testing specialist ran `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android`; build succeeded with 0 warnings and 0 errors. |
| acceptance | `passed` | Required API names and limitations were documented in `docs/ausbc-binding.md`; runtime/hardware validation is deferred to later Wave 2 tasks. |
| security | `passed` | Cybersecurity specialist found no secrets, API keys, device serials, full environment dumps, or unsafe logging guidance introduced by this task. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Generated API is unusable or incomplete | AUSBC specialist | Add minimal transforms or escalate to updated AAR/dependency artifact. |
| Android 16 page-size warnings remain | AUSBC specialist | Document warning; do not claim Android 16 readiness. |

## Completion report

- What changed: `docs/ausbc-binding.md` was updated with binding build results, generated API surface findings, callback/session types, native library inventory, limitations, and next integration notes. `Transforms/Metadata.xml` was not changed because no binding defect required it.
- Agents used: `ausbc-android-binding-specialist`, `testing-specialist`, `cybersecurity-specialist`.
- Skills used: `dotnet-android-ausbc-binding`, `csharp-developer`, `systematic-debugging`, `verification-before-completion`, `cybersecurity`.
- Validation executed: `dotnet build src/Vision2Audio.AusbcBinding/Vision2Audio.AusbcBinding.csproj -f net10.0-android` succeeded with 0 warnings and 0 errors during testing-specialist validation.
- Validation not executed and why: Android 11 OTG/UVC hardware validation, APK/AAB package inspection, and runtime preview/capture behavior were not executed because they belong to later Wave 2 tasks.
- Context updates completed or needed: No context update required for this task beyond the previously created Wave 2 planning context.
- Documentation updates completed or needed: `docs/ausbc-binding.md` updated with API findings and limitations.
- Remaining risks: Hardware behavior remains unproven; Android 16 16 KB page-size readiness is not claimed; native AAR provenance/hashes remain a broader supply-chain follow-up.
- Recommended next step: Execute `vision2audio-2.3` to implement the Android USB Host/AUSBC session boundary.

## Guardrails

- Do not edit generated binding files directly.
- Do not expand into app integration before binding readiness is known.
