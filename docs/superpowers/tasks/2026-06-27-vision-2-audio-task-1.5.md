# Task: Validation, acceptance evidence, and context updates

## Metadata

- Task ID: `vision2audio-1.5`
- Related Wave ID: `vision2audio-wave-1`
- Related Spec ID: `2026-06-27-vision-2-audio-design`
- Status: `draft`
- Size: `M`
- Category: `standard`
- Created: `2026-06-27`
- Updated: `2026-06-27`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Spec reference: `Context updates 133-143; Planning readiness 144-153`
- Wave file: `docs/superpowers/waves/2026-06-27-vision-2-audio-wave-1.md`
- Wave task row/reference: `vision2audio-1.5`

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

Close the wave with acceptance evidence, validation results, and the required context/documentation updates.

## Expected result

The approved criteria are documented as met, and the project context files reflect the completed implementation state.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- End-to-end validation evidence — source: `spec`
- Acceptance criteria review against the approved spec — source: `spec`
- Update required context files with current implementation state — source: `spec`

## Out of scope

- New feature work
- Scope changes or post-wave enhancements

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `vision2audio-testing-specialist`: validation evidence and regression coverage.
- `vision2audio-acceptance-specialist`: acceptance review and release-readiness check.
- `context-maintainer.md` or `docs-maintainer.md`: durable context updates.

### Recruited agents

- None beyond the specialists above.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `verification-before-completion` | `vision2audio-testing-specialist`, `vision2audio-acceptance-specialist` | Require evidence before completion claims. |
| `systematic-debugging` | `vision2audio-testing-specialist` | Triage validation failures cleanly if any appear. |

## Files or areas expected to change

- Context files under `.opencode/context/`
- Wave/task completion notes
- Validation evidence artifacts if produced

## Validation plan

- Build/check command: `dotnet build`
- Test command: `dotnet test`
- Manual validation: confirm the full approved user flow on Android hardware and record the outcomes.
- Security validation: confirm no sensitive data, secrets, or unsafe logs remain in the completed flow.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending` | Review wave result against the spec. |
| tests | `pending` | Record unit/integration/device validation results. |
| acceptance | `pending` | Confirm all spec acceptance criteria are satisfied or explicitly deferred. |
| security | `pending` | Confirm the direct-to-OpenAI path stays within approved risk bounds. |

## Execution notes

- Close the wave only after the evidence is written down.

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Missing or weak validation evidence | Testing specialist | Re-run the failed path and capture the result. |

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
