# Task: <task-title>

## Metadata

- Task ID: `<task-id>`
- Related Wave ID: `<wave-id>`
- Related Spec ID: `<spec-id>`
- Status: `draft | ready-for-execution | in-progress | blocked | completed | superseded`
- Size: `XS | S | M | L`
- Category: `standard | XS-doc-only | XS-safe-change`
- Created: `<yyyy-mm-dd>`
- Updated: `<yyyy-mm-dd>`

## Spec traceability

- Spec file: `<path-to-spec>`
- Spec reference: `<section / acceptance criterion>`
- Wave file: `<path-to-wave>`
- Wave task row/reference: `<reference>`

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

Describe the task objective.

## Expected result

Describe the expected outcome.

## In scope

Each item must trace back to the approved spec or human-approved ad hoc request.

- `<item>` — source: `<spec/wave/human approval>`

## Out of scope

- `<item>`

## Required agents

### Coordinator

- `orchestrator.md` or `<human/direct executor>`

### Active subagents

- `<agent>`: `<reason>`

### Recruited agents

- `<agent>`: `<source blueprint>` / `<reason>`

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `<skill>` | `<agent>` | `<reason>` |

## Files or areas expected to change

- `<path-or-area>`

## Validation plan

- Build/check command: `<command or not-applicable>`
- Test command: `<command or not-applicable>`
- Manual validation: `<steps or not-applicable>`
- Security validation: `<steps or not-applicable>`

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `pending | passed | failed | not-applicable | waived` | `<evidence>` |
| tests | `pending | passed | failed | not-applicable | waived` | `<evidence>` |
| acceptance | `pending | passed | failed | not-applicable | waived` | `<evidence>` |
| security | `pending | passed | failed | not-applicable | waived` | `<evidence>` |

## Execution notes

- `<note>`

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| `<risk>` | `<owner>` | `<next action>` |

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
