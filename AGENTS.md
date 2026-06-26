# Golden-Dolphin AI Coding Guide

NOTE: Team members can create AGENTS.local.md for personal overrides — it's gitignored. Global settings can go in ~/.codex/AGENTS.md or ~/.claude/CLAUDE.md.

## Project Overview

Golden-Dolphin is the Unity project for 塔楼牌 Turrot, a 2D card-and-collapse game built with Unity 6000.3.18f1 and URP 2D.

The current demo is the Memory Tower / 记忆危楼 MVP. Keep changes aligned with the playable slice documented in:

- Local GDD/MVP docs: `Assets/_Project/Docs/00_MVP_Demo_目标与风格.md` and `Assets/_Project/Docs/01_玩法规则与数值表.md`
- GitHub Wiki GDD: `https://github.com/forest27xx/Golden-Dolphin/wiki/GDD`
- GitHub Wiki AGENTS.md: `https://github.com/forest27xx/Golden-Dolphin/wiki/AGENTS.md`

## Core Systems

Keep the gameplay architecture split into three decoupled systems:

- Cards: card configs, deck/hand flow, target rules, and effect resolution.
- Building: block data, grid layout, block HP/state, and visual block representation.
- Collapse: collapse pressure, support checks, unstable states, and collapse resolution.

Avoid mixing UI concerns, card rules, building mutation, and collapse checks in one class. Use explicit data passed between systems instead of hidden scene lookups.

## Architecture Conventions

- Use the `MemoryTower` namespace for runtime code.
- Editor-only helpers belong under an editor namespace such as `MemoryTower.EditorTools`.
- Prefer small model/service classes for rules, with MonoBehaviours acting as scene bindings and view controllers.
- Keep configs data-driven. `BuiltInConfigs` is a transitional fallback; move cards, levels, block definitions, tuning numbers, and art references toward ScriptableObject assets or JSON-backed data under `Assets/_Project/Data`.
- Do not add new long-lived hardcoded tables to runtime code when a ScriptableObject or JSON config would be practical.
- Keep IDs stable for cards, levels, block types, saves, and resources.

## MVP Scope

The MVP target is intentionally small:

- 5 levels: tutorial, 3 normal levels, and a final core level.
- 8+ cards: keep the existing base set working before adding new cards.
- A complete loop: choose card, target block when required, resolve damage/collapse/fragments, then win or lose cleanly.

Do not expand scope unless the task explicitly asks for it. Favor fixes that make the current slice more readable, tunable, and stable.

## Coding Rules

- Follow Unity 2D practices: use 2D physics/colliders when physics is needed, Sorting Layers/Order in Layer for draw order, and URP 2D lights/materials only where they serve the current art direction.
- Prefer `SpriteRenderer` for in-game world objects. Use Canvas/uGUI for HUD, menus, overlays, and debug tooling, not for core in-world gameplay objects unless maintaining existing UI code.
- Keep per-frame work cheap: avoid unnecessary allocations in `Update`, cache component references, and avoid scene-wide searches in hot paths.
- Use `[SerializeField] private` fields for inspector wiring instead of public mutable fields.
- Preserve `.meta` files for every Unity asset. Never commit generated `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, or `UserSettings/` content.
- Keep scene and prefab edits focused. Mention scene/prefab changes clearly in PRs because Unity YAML conflicts are expensive to resolve.
- AI-generated code must be reviewed by a human and verified in Unity before merge.

## Git Workflow

- Branch from the current integration branch used by the team.
- Commit only relevant files for the task.
- Track project files that make the Unity project reproducible: `Assets/`, `Packages/`, `ProjectSettings/`, docs, scripts, `.gitignore`, `.gitattributes`, `AGENTS.md`, `CLAUDE.md`, `README.md`, and `CONTRIBUTING.md`.
- Keep personal AI settings in `AGENTS.local.md` or `CLAUDE.local.md`; these files are intentionally ignored.
