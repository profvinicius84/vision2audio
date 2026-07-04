# Task: Triggered capture, GPS context, and direct OpenAI request

## Metadata

- Task ID: `vision2audio-1.3`
- Related Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `draft`
- Size: `L`
- Category: `standard`
- Created: `2026-06-27`
- Updated: `2026-06-27`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Scope 43-46; Acceptance 66-76`
- Wave file: `docs/superpowers/waves/2026-06-27-vision-2-audio-wave-1.md`
- Wave task row/reference: `vision2audio-1.3`

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

Implement the approved physical trigger path, capture an image from the OTG camera, read current GPS coordinates, and send the image plus context directly to OpenAI.

## Expected result

When the approved trigger is used, the app captures a scene, includes GPS context, and successfully sends the request without any backend hop.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- Bluetooth remote / keyboard trigger handling — source: `spec`
- OTG camera capture — source: `spec`
- GPS coordinate retrieval for each capture — source: `spec`
- Direct OpenAI request composition and response receipt — source: `spec`

## Out of scope

- Response rendering and speech playback details
- Offline warning UI
- Backend or proxy services

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `vision2audio-stack-specialist`: Android/MAUI device integration.
- `vision2audio-frontend-specialist`: trigger flow and user feedback hooks.
- `vision2audio-cybersecurity-specialist`: direct API and secret-handling review.

### Recruited agents

- None beyond the three specialists above.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `csharp-developer` | `vision2audio-stack-specialist`, `vision2audio-frontend-specialist` | Implement Android/MAUI capture flow. |
| `dotnet-csharp-configuration` | `vision2audio-stack-specialist` | Handle approved runtime config and secret-bound settings. |
| `csharp-async-patterns` | `vision2audio-stack-specialist` | Coordinate capture, GPS, and network calls safely. |
| `cybersecurity` | `vision2audio-cybersecurity-specialist` | Reduce risk from direct OpenAI usage and logging. |

## Files or areas expected to change

- Camera integration and device input handling
- GPS/location service
- OpenAI request builder / client
- Trigger command or event wiring

## Validation plan

- Build/check command: `dotnet build`
- Test command: `dotnet test`
- Manual validation: press the approved trigger on Android hardware, confirm the app captures, reads GPS, and sends the request directly.
- Security validation: verify no secret is logged or shipped in plain text, and confirm permission usage is minimal.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending` | Review trigger path, camera flow, and request composition. |
| tests | `pending` | Unit/integration tests for request assembly and trigger handling. |
| acceptance | `pending` | Capture request succeeds with OTG camera and GPS context. |
| security | `pending` | Direct API access remains within approved client-only constraints. |

## Execution notes

- Keep the request path direct, explicit, and minimal.

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Device hardware variability | Stack specialist | Validate on representative Android hardware early. |
| Prompt/request contract drift | Frontend specialist | Keep request payload aligned with the approved spec. |

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
