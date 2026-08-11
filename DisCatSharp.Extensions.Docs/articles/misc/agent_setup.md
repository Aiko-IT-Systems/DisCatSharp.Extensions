---
uid: misc_agent_setup
title: AI Coding Assistants
description: Install the DisCatSharp Agent Skill and connect the shared documentation MCP for the core library and official Extensions.
---

# AI coding assistants

DisCatSharp provides one version-aware skill for projects using the core library and its official Extensions. The canonical skill lives in the main DisCatSharp repository so clients receive one consistent set of instructions instead of separate, potentially conflicting copies.

## Install the skill

Install `use-discatsharp` globally for Codex:

```shell
gh skill install Aiko-IT-Systems/DisCatSharp use-discatsharp --agent codex --scope user --pin main
```

Omit `--pin main` to follow the latest stable DisCatSharp release after the skills ship in a stable version. You can also pin the same tag as the DisCatSharp version used by your application.

For Copilot, Claude Code, Gemini CLI, generic Agent Skills clients, and alternative installation methods, see the [complete skill installation guide](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/skills/README.md#install).

## Connect the documentation MCP

The public documentation MCP covers both the main DisCatSharp library and official Extensions:

```text
https://docs.dcs.aitsys.dev/mcp
```

It is public, stateless, and does not require authentication. Clients can search both documentation corpora together or restrict a lookup to the `extensions` corpus. The skill teaches supported agents when to query it and how to fall back when MCP is unavailable.

See the [MCP client examples](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/skills/README.md#connect-the-documentation-mcp) for Codex, Copilot, Claude Code, Gemini CLI, and generic clients.
