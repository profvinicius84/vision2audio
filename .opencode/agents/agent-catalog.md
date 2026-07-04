# SDD Specialist Agents Catalog

This directory defines the agent roles used by the SDD Toolkit.

The human-facing agents are `product-owner.md`, `env-configr.md`, `tech-lead.md` and `orchestrator.md`. Other agents are subagents called only within their allowed invocation boundaries.

Use `agent-blueprints/` when a project needs stack-specific implementation, repository or project-management agents.

## Human-facing agents

- `product-owner.md`: configures the project with the human, clarifies product goals, identifies definition gaps, asks required setup questions, owns product-context decisions and calls allowed subagents to record approved setup work. It must use the `grill-me` skill for product clarification and requirement validation. It must not infer technical details and must not call Tech Lead or Orchestrator.
- `env-configr.md`: configures the development and AI-agent environment with the human or Product Owner, including AI platform, MCPs, readiness rules, model routing, communication rules, setup scripts and environment-specific instructions for Agent Recruiter and Skill Builder.
- `tech-lead.md`: can only be invoked by a human. It plans approved saved specs into coherent saved waves and tasks with consistent dependencies, required agents, skills and validation. It must use the `grill-me` skill for planning conversations. It delegates planned execution to the Orchestrator.
- `orchestrator.md`: can only be invoked by Tech Lead or by a human. It coordinates execution for planned tasks from the Tech Lead and explicit human-approved ad hoc tasks. It identifies capable agents, calls them, enforces artifact persistence, document consistency and quality gates.

## Invocation boundaries

- Tech Lead can only be invoked by a human.
- Orchestrator can only be invoked by Tech Lead or by a human.
- Product Owner must not call Tech Lead.
- Product Owner must not call Orchestrator.
- Env Configr, Spec Writer, Context Maintainer, Docs Maintainer and other subagents must not call Tech Lead or Orchestrator.
- If an agent detects that technical planning is required, it must tell the human to call Tech Lead.
- If an agent detects that execution coordination is required, it must tell the human whether the next step is Tech Lead planned execution or human-approved ad hoc Orchestrator invocation.

## Artifact persistence

Every durable artifact must be saved to disk before related work is reported as complete.

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

Completion reports for artifact-producing work must include saved file paths or clearly state why no file was created.

## Coordination subagents

- `context-maintainer.md`: keeps the `context/` folder accurate, concise and consistent using approved or clearly sourced product, business, architecture, stack, decision, glossary, constraint and current-state information.
- `spec-writer.md`: writes formal SDD specifications from requirements approved by the Product Owner and the human stakeholder. It must save every generated spec to disk.
- `agent-recruiter.md`: recruits and configures project-specific implementation agents from `agent-blueprints/`.
- `skill-builder.md`: creates, adapts or recommends skills required by recruited agents, using official documentation, web research and optionally skills.sh with a user-provided token.

## Quality gate subagents

- `review-specialist.md`: reviews diffs, scope, maintainability, validation evidence and readiness for human review or merge.
- `testing-specialist.md`: defines and implements automated testing strategy across unit, integration, regression, smoke and end-to-end tests.
- `acceptance-specialist.md`: validates acceptance criteria, exploratory scenarios, release risk and product readiness.
- `cybersecurity-specialist.md`: performs security pre-checks and final security review for tasks with security impact.

## Documentation subagents

- `docs-maintainer.md`: reviews and maintains human-facing documentation, specs, links, workflow notes and guides. It should be called at the end of each wave before closure and must save documentation updates to disk.

## General technical subagents

- `architecture-specialist.md`: reviews system boundaries, module responsibilities, dependencies, public contracts and long-term maintainability.
- `ux-specialist.md`: reviews user flows, screen behavior, interaction states, copy clarity, accessibility concerns and developer experience.

## Project-specific implementation agents (Vision 2 Audio)

- `maui-android-stack-specialist.md`: implements the Android .NET MAUI client stack for OTG camera capture, GPS, direct OpenAI calls, text-to-speech and voice-first UI wiring. Primary skills: `csharp-developer`, `csharp-async-patterns`, `dotnet-csharp-dependency-injection`, `dotnet-csharp-configuration`, `dotnet-csharp-nullable-reference-types`, `dotnet-csharp-modern-patterns`, `modern-csharp`, `writing-csharp-code`.
- `local-history-data-specialist.md`: implements local read-only history persistence, clear-all behavior, SQLite storage and migration handling. Primary skills: `sqlite`, `csharp-developer`, `csharp-async-patterns`, `dotnet-csharp-dependency-injection`, `dotnet-csharp-configuration`, `dotnet-csharp-nullable-reference-types`, `modern-csharp`, `writing-csharp-code`.

## Agent blueprints

Blueprints are not the default active implementation team. They are templates used by `agent-recruiter.md` to create project-specific agents.

Available blueprints:

- `agent-blueprints/stack-specialist.md`: base template for a concrete technology stack or framework specialist.
- `agent-blueprints/frontend-specialist.md`: template for a project-specific frontend implementation agent.
- `agent-blueprints/backend-specialist.md`: template for a project-specific backend implementation agent.
- `agent-blueprints/api-specialist.md`: template for a project-specific API implementation agent.
- `agent-blueprints/data-specialist.md`: template for a project-specific data implementation agent.
- `agent-blueprints/devops-specialist.md`: template for a project-specific DevOps implementation agent.
- `agent-blueprints/repository-specialist.md`: template for a project-specific repository operations agent for GitHub, Azure Repos, SVN, VSS or another source control system.
- `agent-blueprints/project-management-specialist.md`: template for a project-specific task and project management operations agent for Azure Boards, GitHub Issues, Redmine, Jira or another project management system.

## Document templates

Use templates to keep specs, waves and tasks traceable:

- `docs/templates/spec-template.md`: formal SDD specification template.
- `docs/templates/wave-template.md`: wave planning template with spec traceability and planning preconditions.
- `docs/templates/task-template.md`: executable task template with spec/wave traceability, agents, skills, validation and gates.

## Configuration templates

Use configuration templates to adapt SDD Toolkit to the project and platform:

- `config/model-routing.example.json`: model profiles, routing rules and communication defaults.
- `config/mcp-config.example.json`: MCP server policy, readiness and safety rules.
- `config/readiness-matrix.example.json`: stage-based definition readiness matrix.

## Context folder

The `context/` folder stores agent-facing operational context:

- `context/workflow.md`
- `context/product.md`
- `context/business-rules.md`
- `context/architecture.md`
- `context/stack.md`
- `context/decisions.md`
- `context/glossary.md`
- `context/constraints.md`
- `context/current-state.md`

Product Owner owns product-context decisions. Context Maintainer keeps the context folder accurate, concise and consistent.

## Readiness and blocking rules

Not every definition is required at project start.

Definitions and configurations should be classified as:

- `mandatory_now`: required before the current stage can proceed.
- `optional_now`: useful now, but not required to proceed.
- `later_stage`: not required now; required only when a later stage or task needs it.
- `not_applicable`: not required for this project or task.

Examples:

- Deployment environment is not required during early project setup, but is mandatory when deploy is requested.
- Database credentials are not required during early project setup, but are mandatory when a task must connect to a real database.
- MCP servers are optional at project start, but mandatory when a task requires external access through MCP.

When a missing definition blocks progress, the active agent must tell the human which definition is missing, why it is required now, which stage or task it blocks and which agent should resolve it.

## Communication rules

- Inter-agent communication defaults to English.
- Specialized agents communicating with models should use the `caveman` skill by default to reduce token usage while preserving technical accuracy.
- Human interaction must adapt to the human's language.
- Human-facing artifacts must follow the language expected by the human or project context.
- Do not force English for human-facing artifacts unless the project explicitly requires it.
- Do not use caveman compression with humans unless the human explicitly asks for it.

## Recommended flow

1. Product Owner asks the human the questions needed to configure the project.
2. Product Owner identifies definition gaps and calls Env Configr for environment, AI platform, MCP, readiness, model routing and communication setup.
3. Env Configr classifies missing definitions as mandatory now, optional now, later stage or not applicable.
4. Env Configr calls Agent Recruiter and Skill Builder with platform-specific and environment-specific instructions when needed.
5. Product Owner calls allowed subagents to record approved setup work.
6. Product Owner debates and approves requirements with the human stakeholder.
7. Product Owner calls Context Maintainer when product or business context changes.
8. Product Owner calls Spec Writer.
9. Spec Writer writes and saves the SDD specification using `docs/templates/spec-template.md`.
10. Product Owner tells the human that the saved spec is ready for Tech Lead planning.
11. Human calls Tech Lead.
12. Tech Lead verifies the approved saved spec, stack, required agents and required skills before planning.
13. Tech Lead plans and saves waves and tasks using `docs/templates/wave-template.md` and `docs/templates/task-template.md`.
14. Tech Lead validates dependencies and keeps project/task-management systems consistent by calling the recruited specialist when integration exists.
15. Tech Lead calls Context Maintainer when planning creates or reveals durable technical context.
16. Tech Lead calls Agent Recruiter when implementation requires stack-specific, repository or project-management agents.
17. Agent Recruiter asks Skill Builder which skills recruited agents need.
18. Skill Builder researches, recommends or drafts required skills.
19. Agent Recruiter creates or configures agents from blueprints and assigns required skills.
20. Tech Lead calls Orchestrator with saved task details when planned execution is needed.
21. Orchestrator identifies capable agents, calls them and coordinates execution.
22. Orchestrator may also coordinate explicit human-approved ad hoc tasks when called by a human and scope/risk are clear.
23. Quality gate subagents validate `review`, `tests`, `acceptance` and `security`.
24. Orchestrator ensures document consistency, artifact persistence and quality gates.
25. Context Maintainer updates current state when relevant.
26. Docs Maintainer updates human-facing documentation before wave closure.

## Mandatory rules

- Product Owner, Env Configr, Tech Lead and Orchestrator are available to humans.
- All other active agents are subagents.
- Tech Lead can only be invoked by a human.
- Orchestrator can only be invoked by Tech Lead or by a human.
- Product Owner must use the `grill-me` skill for product clarification and requirement validation.
- Tech Lead must use the `grill-me` skill for planning conversations.
- Product Owner must not call Tech Lead.
- Product Owner must not call Orchestrator.
- Env Configr and subagents must not call Tech Lead or Orchestrator.
- Product Owner must ask for missing project definitions and must not infer technical details.
- Product Owner must call Env Configr during early project setup when environment, AI platform, MCPs, readiness, model routing or communication rules are undefined.
- Product Owner must notify the human when a saved spec is ready for Tech Lead planning.
- Env Configr must not guess models, subscription features, platform capabilities, MCP server capabilities or credentials.
- Env Configr must classify missing definitions by readiness level before blocking a stage.
- Env Configr must pass environment, platform, MCP and readiness instructions to Agent Recruiter and Skill Builder when they are called.
- MCPs must not block early project setup unless a current task requires them.
- Credentials must never be stored in repository files.
- Inter-agent communication must default to English.
- Agents should use the `caveman` skill for inter-agent and specialized model communication by default.
- Human interaction and human-facing artifacts must adapt to the human's language.
- Every durable artifact must be saved to disk before completion is reported.
- Spec Writer must save specs to disk.
- Tech Lead must not plan from chat-only specs when a durable spec file is expected.
- Tech Lead must save waves and tasks to disk.
- Tech Lead must not plan work outside the approved spec.
- Tech Lead must not start planning until stack, required agents and required skills are defined or explicitly not needed.
- Tech Lead must keep task dependencies consistent and coherent.
- Tech Lead must call Orchestrator to execute planned tasks.
- Orchestrator must reject invocation from any agent except Tech Lead.
- Orchestrator must identify capable agents and call them.
- Orchestrator must enforce document consistency, artifact persistence and quality gates.
- Product Owner approves requirements with the human before calling Spec Writer.
- Product Owner owns product-context decisions.
- Context Maintainer maintains `context/`; it does not approve product decisions.
- Spec Writer writes specs; it does not approve requirements or plan implementation tasks.
- Planning agents do not edit code.
- Agent Recruiter creates or configures agents; it does not implement project features.
- Skill Builder creates, adapts or recommends skills; it does not implement project features or recruit agents.
- Skill Builder may use skills.sh only when the user provides their own authentication token for the current task.
- Every task must report `review`, `tests`, `acceptance` and `security` gates.
- Lightweight task categories may simplify checklists, but do not remove gates.
- Tasks with potential security impact must call `cybersecurity-specialist.md` at the beginning and end.
- Tasks with documentation changes must call `testing-specialist.md` and `review-specialist.md` at the end.
- Parallel execution is allowed only when tasks have no dependency, no shared critical files and no meaningful impact overlap.

## Review principle

Every specialist should leave work easier for a human to review: clear scope, small diff, explicit validation, saved artifacts and honest notes about uncertainty or known failures.
