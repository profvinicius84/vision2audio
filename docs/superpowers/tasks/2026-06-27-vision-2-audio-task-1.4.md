# Task: Portuguese response UI, speech playback, and offline warning

## Metadata

- Task ID: `vision2audio-1.4`
- Related Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `draft`
- Size: `M`
- Category: `standard`
- Created: `2026-06-27`
- Updated: `2026-06-27`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Scope 48-51; Acceptance 70-80; NFR 127-129`
- Wave file: `docs/superpowers/waves/2026-06-27-vision-2-audio-wave-1.md`
- Wave task row/reference: `vision2audio-1.4`

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

Show the OpenAI response on screen in Brazilian Portuguese, speak it automatically, and block capture with a clear warning when internet is unavailable.

## Expected result

The user sees the returned description, hears it read aloud, and receives a no-connection warning instead of a failed request when offline.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- Brazilian Portuguese response display — source: `spec`
- Automatic text-to-speech playback — source: `spec`
- No-connection warning and request suppression — source: `spec`
- Connection-state feedback that keeps the flow accessible — source: `spec`

## Out of scope

- Capture or GPS implementation details
- History persistence internals
- Any backend/offline generation fallback

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `vision2audio-frontend-specialist`: UI, accessibility, and response presentation.
- `vision2audio-testing-specialist`: validation of online/offline and speech flows.
- `vision2audio-acceptance-specialist`: acceptance criteria checks.

### Recruited agents

- None beyond the three specialists above.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `csharp-developer` | `vision2audio-frontend-specialist` | Bind response and warning states into the MAUI UI. |
| `csharp-async-patterns` | `vision2audio-frontend-specialist` | Handle async response and speech lifecycles. |
| `verification-before-completion` | `vision2audio-testing-specialist`, `vision2audio-acceptance-specialist` | Verify user-visible behavior before closure. |

## Files or areas expected to change

- Response view / page / view-model
- Text-to-speech integration
- Connectivity check / offline state handling
- Accessibility copy and layout refinements

## Validation plan

- Build/check command: `dotnet build`
- Test command: `dotnet test`
- Manual validation: run the app on Android, complete a successful capture, confirm the text is in Brazilian Portuguese, verify speech playback, then disconnect internet and confirm capture is blocked with a clear warning.
- Security validation: ensure the warning and logs do not leak secrets or request payload details.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending` | Review UI clarity, speech lifecycle, and offline behavior. |
| tests | `pending` | Offline/online and presentation tests pass. |
| acceptance | `pending` | Brazilian Portuguese text, auto-speech, and no-connection warning all work. |
| security | `pending` | No sensitive information exposed in UI or logs. |

## Execution notes

- Keep the visible text and spoken output aligned.

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| TTS timing or state mismatch | Frontend specialist | Tie speech lifecycle to the captured response state. |

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
