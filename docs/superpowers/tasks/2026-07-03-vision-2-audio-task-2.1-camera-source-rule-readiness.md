# Task: Reconcile camera-source rule and binding readiness

## Metadata

- Task ID: `vision2audio-2.1`
- Related Wave ID: `vision2audio-wave-2-camera-source-hardening`
- Related Spec ID: `2026-06-29-camera-preview-panel-design; 2026-07-01-camera-selection-select-design`
- Status: `completed`
- Size: `S`
- Category: `standard`
- Created: `2026-07-03`
- Updated: `2026-07-03`

## Spec traceability

- Spec file: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Spec reference: `Business rules 80-82`
- Spec file: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Spec reference: `Scope 42-48; Business rules 81-84`
- Wave file: `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`
- Wave task row/reference: `vision2audio-2.1`

## Execution preconditions

- [x] Task is traceable to an approved spec.
- [x] Task is included in an approved wave plan.
- [x] Stack context is defined.
- [x] Required agents are defined.
- [x] Required skills are defined or not applicable.
- [x] Expected validation is defined.
- [x] Risks are documented.

## Objective

Make the current camera-source rule explicit before further AUSBC work: user selection is authoritative, and fallback is used when the selected source is unavailable.

## Expected result

Planning/context artifacts clearly state that the camera-selection spec supersedes the older OTG-first default, and AUSBC execution starts from the correct rule.

## In scope

- Record camera-source precedence in context/decision artifacts — source: `camera-selection spec`.
- Confirm the AUSBC binding task is split and no longer executed as one large task — source: `wave 2`.
- Confirm validation commands and agent names match current project context — source: `.opencode/context/stack.md` and available agents.

## Out of scope

- Code changes.
- Hardware validation.

## Required agents

### Coordinator

- `orchestrator.md`

### Active subagents

- `architecture-specialist.md`: rule consistency and dependency review.
- `acceptance-specialist.md`: acceptance traceability.
- `docs-maintainer.md` or `context-maintainer.md`: durable documentation updates.

### Recruited agents

- None.

## Required skills

| Skill | Used by agent | Reason |
| --- | --- | --- |
| `verification-before-completion` | `acceptance-specialist` | Require evidence before marking planning ready. |

## Files or areas expected to change

- `.opencode/context/decisions.md`
- `.opencode/context/architecture.md`
- `.opencode/context/current-state.md`
- `docs/superpowers/tasks/2026-07-03-vision-2-audio-task-ausbc-otg-binding.md`

## Validation plan

- Build/check command: `not-applicable; planning/context task`
- Test command: `not-applicable; planning/context task`
- Manual validation: inspect updated artifacts and confirm the precedence rule and split-task references are present.
- Security validation: ensure no secrets or device identifiers are added to documentation.

## Quality gates

| Gate | Status | Evidence / justification |
| --- | --- | --- |
| review | `passed` | Architecture specialist confirmed camera precedence, AUSBC split, boundaries, and validation planning are consistent. |
| tests | `not-applicable` | Documentation-only planning reconciliation. |
| acceptance | `passed` | Acceptance specialist confirmed traceability intent: user selection supersedes OTG-first and Wave 2 split is documented. |
| security | `passed` | Acceptance/security observation found no secrets, device identifiers, API keys, or environment dumps introduced. |

## Risks and blockers

| Risk/blocker | Owner | Next action |
| --- | --- | --- |
| Product disagreement about camera precedence | Human / Product Owner | Clarify before executing AUSBC work. |

## Completion report

- What changed: Normalized task agent names to existing `.md` agent file names and recorded completion gate evidence for task 2.1.
- Agents used: `architecture-specialist.md`, `acceptance-specialist.md`.
- Skills used: `spec-to-task-plan` in Tech Lead planning flow; specialists reported evidence-first review results.
- Validation executed: Inspected Wave 2, task 2.1, superseded AUSBC task, camera-preview and camera-selection specs, and context files for camera-source precedence and Wave 2 split consistency.
- Validation not executed and why: Build and tests were not run because this is a planning/context-only reconciliation task.
- Context updates completed or needed: Already completed in `.opencode/context/decisions.md`, `.opencode/context/architecture.md`, `.opencode/context/current-state.md`, and `.opencode/context/stack.md` before closure.
- Documentation updates completed or needed: `docs/ausbc-binding.md`, Wave 2, and the superseded AUSBC task already point to the split Wave 2 plan.
- Remaining risks: Product disagreement about camera precedence would require Product Owner clarification, but no such disagreement was found during review.
- Recommended next step: Execute `vision2audio-2.2` to inspect and stabilize the AUSBC binding API surface.

## Guardrails

- Do not implement code in this task.
- Do not change product behavior beyond approved specs.
