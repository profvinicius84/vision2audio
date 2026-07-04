# Task: MAUI Android app shell and shared contracts

## Metadata

- Task ID: `vision2audio-1.1`
- Related Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `draft`
- Size: `M`
- Category: `standard`
- Created: `2026-06-27`
- Updated: `2026-06-27`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Scope 42-53; Constraints 97-105`
- Wave file: `docs/superpowers/waves/2026-06-27-vision-2-audio-wave-1.md`
- Wave task row/reference: `vision2audio-1.1`

## Execution preconditions

Execution may begin only when all are checked:

- [ ] Task is traceable to an approved spec.
- [ ] Task is included in an approved wave plan or explicitly approved as an ad hoc task by the human.
- [ ] Stack context is defined or not applicable.
- [ ] Required agents are defined.
- [ ] Required skills are defined or not applicable.
- [ ] Expected validation is defined.
- [ ] Risks are documented.

## Objective

Create the Android .NET MAUI app foundation, shared service contracts, and accessibility-ready shell needed for the capture flow.

## Expected result

A runnable MAUI app shell exists with the core abstractions wired for capture, location, OpenAI request handling, speech output, and history storage.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- Android .NET MAUI app shell — source: `spec`
- Shared service/contracts for capture, location, request, speech, and history — source: `spec`
- Accessibility-first basic navigation/layout baseline — source: `spec`

## Out of scope

- Actual camera/GPS/OpenAI implementation
- Local history persistence details
- Validation harness beyond basic app build/run

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `vision2audio-stack-specialist`: .NET MAUI and Android conventions.
- `vision2audio-frontend-specialist`: app shell and accessible UI baseline.

### Recruited agents

- None beyond the two specialists above.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `csharp-developer` | `vision2audio-stack-specialist`, `vision2audio-frontend-specialist` | MAUI app structure and idiomatic implementation. |
| `modern-csharp` | `vision2audio-stack-specialist`, `vision2audio-frontend-specialist` | Keep the foundation maintainable. |
| `dotnet-csharp-dependency-injection` | `vision2audio-stack-specialist` | Wire shared services cleanly. |
| `dotnet-csharp-configuration` | `vision2audio-stack-specialist` | Prepare approved runtime configuration hooks. |

## Files or areas expected to change

- MAUI solution/project files
- App bootstrap and dependency registration
- Shared service interfaces / models
- Basic shell or main page layout

## Validation plan

- Build/check command: `dotnet build`
- Test command: `not applicable yet; build-focused foundation task`
- Manual validation: launch the Android app shell, verify the UI renders, and confirm the app starts without missing registrations.
- Security validation: ensure no secrets or API keys are hard-coded in the app shell or logging.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending` | Check shell structure, service boundaries, and MAUI conventions. |
| tests | `pending` | Build succeeds; unit tests not expected yet. |
| acceptance | `pending` | Foundation ready for capture/history tasks. |
| security | `pending` | No secrets exposed in bootstrap or config. |

## Execution notes

- Keep the foundation generic enough to support the approved client-only flow.

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Unclear app structure can create rework later | Stack specialist | Keep interfaces small and aligned with the spec. |

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
