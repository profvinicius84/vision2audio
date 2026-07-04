# Wave Plan: <wave-title>

## Metadata

- Wave ID: `<wave-id>`
- Related Spec ID: `<spec-id>`
- Status: `draft | ready-for-execution | in-progress | blocked | completed | superseded`
- Tech Lead: `<name-or-role>`
- Created: `<yyyy-mm-dd>`
- Updated: `<yyyy-mm-dd>`

## Spec traceability

- Spec file: `<path-to-spec>`
- Spec status: `approved`
- Spec version/date: `<version-or-date>`
- Scope source: `<section-or-acceptance-criteria-reference>`

## Planning preconditions

Planning may begin only when all are checked:

- [ ] Spec is approved.
- [ ] Stack is defined in `context/stack.md` or explicitly confirmed for this wave.
- [ ] Required active agents are identified.
- [ ] Required recruited agents are defined or explicitly not needed.
- [ ] Required skills are defined or explicitly not needed.
- [ ] Open questions do not block this wave.

## Wave objective

Describe the outcome this wave should deliver.

## In scope for this wave

Each item must trace back to the approved spec.

| Item | Spec reference | Reason |
| --- | --- | --- |
| `<item>` | `<spec section / acceptance criterion>` | `<reason>` |

## Out of scope for this wave

| Item | Reason |
| --- | --- |
| `<item>` | `<reason>` |

## Required agents

### Active agents

- `<agent>`: `<reason>`

### Recruited agents

- `<agent>`: `<source blueprint>` / `<reason>`

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `<skill>` | `<agent>` | `<reason>` |

## Tasks

| Task ID | Title | Size | Category | Depends on | Spec reference |
| --- | --- | --- | --- | --- | --- |
| `<task-id>` | `<title>` | `XS/S/M/L` | `standard/XS-doc-only/XS-safe-change` | `<task-id or none>` | `<spec reference>` |

## Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| `<risk>` | `<impact>` | `<mitigation>` |

## Quality gate plan

Every task must report:

- `review`
- `tests`
- `acceptance`
- `security`

## Human review points

- `<decision or review point>`

## Completion criteria

- [ ] All tasks completed or explicitly deferred.
- [ ] All gates passed, waived with justification, or not applicable with justification.
- [ ] Context updates completed when needed.
- [ ] Human-facing documentation updated when needed.
- [ ] Human review completed when required.

## Guardrails

- Do not include work that cannot be traced to the approved spec.
- Do not use this wave to expand product scope.
- Do not plan implementation before stack and required agents are defined.
- If scope changes are needed, return to Product Owner and Spec Writer.
