# Configuration

This folder contains optional JSON configuration files for adapting the SDD Toolkit to a project, platform and stage.

## Files

```text
config/model-routing.example.json
config/mcp-config.example.json
config/readiness-matrix.example.json
```

## Model routing

Use `model-routing.example.json` as a starting point for assigning model profiles to agents.

The toolkit does not define fixed model names because model availability depends on:

- the platform being used
- the user's subscription
- the provider configured in the project
- cost limits
- compliance and data policies

The example uses logical profiles:

- `economical`: simple and low-risk work.
- `fast`: routing and simple classification.
- `standard`: balanced day-to-day work.
- `reasoning`: planning, architecture and complex reasoning.
- `high_assurance`: security, acceptance, final review and high-risk decisions.

These are logical profiles, not provider-specific model names.

## MCP configuration

Use `mcp-config.example.json` as a starting point for Model Context Protocol server configuration.

MCPs are not automatically mandatory at project start.

Configure MCPs when they are known or when a task actually needs them, such as:

- repository access through MCP
- task/project-management access through MCP
- live database inspection
- filesystem access
- browser automation
- deployment or environment inspection

Do not store secrets, tokens, database URLs or deployment credentials in repository files.

Use the secure secret mechanism provided by the selected AI platform or runtime.

## Readiness matrix

Use `readiness-matrix.example.json` to decide whether a missing definition blocks the current stage.

Definitions can be classified as:

- `mandatory_now`: required before the current stage can proceed.
- `optional_now`: useful now, but not required to proceed.
- `later_stage`: not required now; becomes required only when a later stage or task needs it.
- `not_applicable`: not required for this project or task.

Examples:

- Deployment environment is not required at project start, but becomes mandatory when deploy is requested.
- Database credentials are not required at project start, but become mandatory when a task must connect to a real database.
- MCP servers are optional at project start, but become mandatory when a task requires external access through MCP.

## Recommended process

1. Copy the example files into the target project.
2. Rename them if the selected platform requires a specific config name.
3. Replace placeholders with project-specific values.
4. Keep credentials outside the repository.
5. Review model routing, MCP availability and readiness rules with the human.
6. Let Env Configr classify missing definitions before blocking work.

## Human decision required

Agents should ask the human instead of guessing when:

- model names are unknown, unavailable or cost-sensitive
- MCP server capabilities are unknown
- credentials are required
- a missing definition may block the current task
- a later-stage configuration becomes necessary
