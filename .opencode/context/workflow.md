# Human x Agents SDD Workflow

This document defines how humans and agents collaborate in the SDD Toolkit.

## Central rule

Do not skip from idea directly to implementation.

Every meaningful change should move through human-guided clarification, environment setup, specification, planning, execution coordination and explicit validation.

Every durable artifact must be saved to disk before the related work is reported as complete.

## Authority model

The human is the final authority for:

- product direction
- requirement approval
- scope decisions
- technical details that are not already defined
- AI platform and environment decisions
- model routing decisions when availability or cost is unclear
- risk acceptance
- third-party skill adoption
- release readiness

Agents assist with clarification, environment setup, writing, planning, recruiting, skill building, execution coordination, quality checks and documentation.

## Human-facing agents

Humans may interact directly with:

- `product-owner.md` for project setup, product clarification, requirement approval flow and readiness checks.
- `env-configr.md` for development environment, AI platform, model routing, setup scripts and communication rules.
- `tech-lead.md` for technical planning, wave/task planning, dependency validation and project/task-management consistency.
- `orchestrator.md` for explicit human-approved ad hoc execution coordination.

All other active agents are subagents.

## Invocation boundaries

- Tech Lead can only be invoked by a human.
- Orchestrator can only be invoked by Tech Lead or by a human.
- Product Owner must not call Tech Lead.
- Product Owner must not call Orchestrator.
- Env Configr, Spec Writer, Context Maintainer, Docs Maintainer and all other subagents must not call Tech Lead or Orchestrator.
- When an agent detects that Tech Lead work is required, it must tell the human to call Tech Lead.
- When an agent detects that execution coordination is required, it must tell the human whether the next step is Tech Lead planned execution or human-approved ad hoc Orchestrator invocation.

## Specialist readiness rules

Product Owner and Tech Lead must both verify specialist readiness before the project moves toward execution.

Product Owner verifies specialist readiness before telling the human that a saved spec is ready for Tech Lead planning.

Tech Lead independently verifies specialist readiness again before planning waves, planning tasks or marking tasks ready for execution.

Required specialists and skills must be either:

- created and available, or
- explicitly marked as not required for the current scope.

Do not allow execution readiness when required specialists or required skills are missing.

## Specialist execution rules

Orchestrator must not implement directly.

Orchestrator must call the appropriate specialists for implementation, validation, review and documentation work.

If a required specialist cannot be called, Orchestrator must not silently proceed. It must report:

- which specialist could not be called
- why the specialist is required
- what prevented the call
- which agent should resolve the gap
- whether the task is blocked or can proceed only in a reduced non-implementation mode

## Agent limit handling

When a called specialist reaches a model limit, context limit, output limit, rate limit or similar agent/model-imposed limit, the calling agent must not immediately propagate that as final failure.

The calling agent must retry by reformulating, narrowing, chunking or splitting the request while the calling agent still has capacity and the task remains safe.

Stop only when:

- the calling agent's own limit is reached
- repeated retries produce no useful progress
- the task becomes unsafe
- required context is missing
- a human decision is required

When stopping, report what was retried, what still failed and the next required action.

## Artifact persistence rules

Agents must save every durable artifact to disk.

Durable artifacts include:

- specs
- wave plans
- task files
- context updates
- decision records
- source files
- test files
- documentation files
- review reports
- validation reports
- project-management export files

Do not treat chat-only output as completed work when a durable artifact is expected.

Every completion report for artifact-producing work must include the saved file path or clearly state why no file was created.

## Communication rules

- Inter-agent communication defaults to English.
- Specialized agents communicating with models should use the `caveman` skill by default to reduce token usage while preserving technical accuracy.
- Human interaction must adapt to the human's language.
- Human-facing artifacts must follow the language expected by the human or project context.
- Do not force English for human-facing artifacts unless the project explicitly requires it.
- Do not use caveman compression with humans unless the human explicitly asks for it.

## Work modes

Before acting, identify the current mode:

```text
project setup
environment configuration
product clarification
context maintenance
specification writing
technical planning
agent recruitment
skill building
execution coordination
quality validation
documentation maintenance
```

## Role separation

### Human stakeholder

Approves product direction, requirement scope, missing project definitions, environment decisions, major risks and release decisions.

### Product Owner

Asks the human the questions needed to configure the project, clarifies product value, identifies definition gaps and calls allowed subagents to record approved setup work.

The Product Owner owns product-context decisions and must notify the human when the project is ready for technical planning.

The Product Owner must not infer technical details. Undefined environment details may be handled by Env Configr. Undefined technical planning decisions require the human to call Tech Lead.

During early project setup, Product Owner calls Env Configr when environment, AI platform, model routing or communication rules are undefined.

Product Owner must not call Tech Lead or Orchestrator.

Product Owner must not say the project is ready for technical planning while required specialists or required skills are missing.

Product Owner must never say the project is ready for execution; execution readiness is controlled later by Tech Lead and Orchestrator rules.

### Env Configr

Configures development environment and AI-agent environment with the human or Product Owner.

It handles AI platform selection, setup script guidance, model routing, communication rules and environment-specific instructions for Agent Recruiter and Skill Builder.

It must not guess models, subscription features, platform capabilities or credentials.

It must not call Tech Lead or Orchestrator.

### Context Maintainer

Maintains the `context/` folder using approved or clearly sourced information.

It does not approve product decisions.

It must save context updates to disk.

### Spec Writer

Writes formal SDD specifications from requirements approved by the Product Owner and the human stakeholder.

It does not approve requirements and does not plan implementation tasks.

It must save every generated spec to disk before reporting completion.

### Tech Lead

Interacts with humans to plan approved specs into coherent waves and executable tasks.

It validates dependencies, required agents, required skills, task order and consistency with project/task-management systems.

To execute a planned task, it calls the Orchestrator with the task details.

The Tech Lead can only be invoked by a human.

The Tech Lead must independently verify specialist readiness before planning or marking tasks ready for execution.

The Tech Lead must save generated wave and task artifacts to disk before reporting completion.

The Tech Lead does not implement code.

### Agent Recruiter

Creates or configures project-specific implementation agents from `agent-blueprints/` when the active agents are not specific enough for the stack.

It receives environment/platform instructions from Env Configr when environment-specific configuration exists.

It does not implement project features.

### Skill Builder

Creates, adapts or recommends skills required by recruited agents.

It may use official documentation, web research and optionally skills.sh only when the user provides their own authentication token for the current task.

It receives environment/platform instructions from Env Configr when environment-specific configuration exists.

It does not recruit agents or implement project features.

### Orchestrator

Coordinates execution of planned tasks received from the Tech Lead and explicit human-approved ad hoc tasks.

It identifies agents capable of executing the task, calls them, coordinates execution, enforces document consistency and ensures quality gates are followed.

The Orchestrator can only be invoked by Tech Lead or by a human.

It must not implement directly and must not execute implementation work without calling required specialists.

It does not bypass gates.

### Quality gate agents

Quality gate agents validate work from different perspectives:

- `review-specialist.md`: technical review and merge readiness.
- `testing-specialist.md`: tests and validation strategy.
- `acceptance-specialist.md`: acceptance criteria and product readiness.
- `cybersecurity-specialist.md`: security impact and residual risk.

### Docs Maintainer

Maintains human-facing documentation, guides, READMEs, release notes and workflow documentation.

It is different from Context Maintainer, which manages agent-facing operational context.

It must save documentation updates to disk.

## Full workflow

```text
idea / issue / request
-> Product Owner asks project setup and product clarification questions
-> Product Owner identifies definition gaps
-> Product Owner calls Env Configr for environment, AI platform, model routing and communication setup
-> human provides or approves missing definitions
-> Context Maintainer records approved context when durable context changes
-> Env Configr calls Agent Recruiter and Skill Builder with environment/platform instructions when needed
-> Product Owner verifies required specialists and skills are created or explicitly not required
-> Product Owner calls Spec Writer after requirement approval
-> Spec Writer writes and saves SDD specification to disk
-> Product Owner tells human that the saved spec is ready for Tech Lead planning
-> human calls Tech Lead
-> Tech Lead verifies saved spec, stack, agents and skills before planning
-> Tech Lead independently verifies specialist readiness
-> Tech Lead plans and saves waves and tasks with coherent dependencies
-> Tech Lead syncs milestones/tasks through project-management specialist when integration exists
-> Tech Lead calls Agent Recruiter when stack-specific agents are needed
-> Agent Recruiter calls Skill Builder when recruited agents need reusable skills
-> Skill Builder researches, recommends or drafts skills
-> Agent Recruiter finalizes recruited agents and skill assignments
-> Tech Lead calls Orchestrator with saved task execution details
-> Orchestrator identifies required specialists and calls them
-> Orchestrator stops and reports missing specialists instead of implementing directly
-> Orchestrator retries/splits specialist calls when a specialist hits model/context limits and Orchestrator capacity remains
-> Orchestrator enforces document consistency, artifact persistence and quality gates
-> Quality gate agents validate review, tests, acceptance and security
-> failed gates trigger correction cycles or replanning
-> human reviews meaningful outputs and risk decisions
-> Context Maintainer updates current state when relevant
-> Docs Maintainer updates human-facing documentation
-> wave or release is considered ready
```

## Ad hoc orchestration

The Orchestrator may receive explicit human-approved ad hoc tasks directly from a human.

Ad hoc tasks must not bypass product or technical governance. If the task changes product scope, architecture, security, data model, release behavior or durable context, the Orchestrator must stop and tell the human that Product Owner or Tech Lead work is required before execution.

Small operational tasks may proceed when scope, risk, expected result and validation are clear.

## Project/task-management integration

When a project-management system is configured, the Tech Lead must keep milestones, waves, tasks and dependencies consistent with the approved plan.

The Tech Lead should call a recruited project-management specialist to update or validate work items, statuses, dependencies, milestones, owners and links to specs, branches and pull requests.

## Quality gates

Every task must report four gates:

- `review`
- `tests`
- `acceptance`
- `security`

Allowed states:

- `passed`
- `failed`
- `not-applicable`, with justification
- `waived`, with justification

A task cannot be finalized while any gate is missing or failed.

## Mandatory rules

- Product Owner, Env Configr, Tech Lead and Orchestrator are available to humans.
- All other active agents are subagents.
- Tech Lead can only be invoked by a human.
- Orchestrator can only be invoked by Tech Lead or by a human.
- Product Owner must not call Tech Lead.
- Product Owner must not call Orchestrator.
- Product Owner must verify specialist readiness before saying a saved spec is ready for Tech Lead planning.
- Product Owner must never say the project is ready for execution.
- Product Owner must ask for missing project definitions and must not infer technical details.
- Product Owner must call Env Configr during early project setup when environment, AI platform, model routing or communication rules are undefined.
- Product Owner must notify the human when a saved spec is ready for Tech Lead planning.
- Env Configr must not guess models, subscription features, platform capabilities or credentials.
- Env Configr must pass environment and platform instructions to Agent Recruiter and Skill Builder when they are called.
- Inter-agent communication must default to English.
- Agents should use the `caveman` skill for inter-agent and specialized model communication by default.
- Human interaction and human-facing artifacts must adapt to the human's language.
- Every durable artifact must be saved to disk before completion is reported.
- Spec Writer must save specs to disk.
- Tech Lead must independently verify specialist readiness before planning or marking tasks ready for execution.
- Tech Lead must not plan from chat-only specs when a durable spec file is expected.
- Tech Lead must save waves and tasks to disk.
- Tech Lead must not plan work outside the approved spec.
- Tech Lead must not start planning until stack, required agents and required skills are defined or explicitly not needed.
- Tech Lead must not mark tasks ready for execution while required specialists or skills are missing.
- Tech Lead must keep task dependencies consistent and coherent.
- Tech Lead must call Orchestrator to execute planned tasks.
- Orchestrator must reject invocation from any agent except Tech Lead.
- Orchestrator must identify capable specialists and call them.
- Orchestrator must not implement directly.
- Orchestrator must not execute implementation work without calling required specialists.
- Orchestrator must report missing or uncallable specialists instead of silently skipping them.
- Calling agents must retry, narrow or split requests when a called agent hits a model/context/rate/output limit, while the calling agent still has capacity.
- Orchestrator must enforce document consistency, artifact persistence and quality gates.
- Every task must report `review`, `tests`, `acceptance` and `security` gates.
