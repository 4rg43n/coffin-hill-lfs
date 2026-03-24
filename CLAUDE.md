# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Coffin Hill** is a horror-themed Pokémon-style roguelite game for PC and Android, built in Unity 6 (6000.3.7f1). The starting point is a clone of Pokémon Red/Blue (1996) with placeholder assets, designed for expansion and refactoring later.

## Unity Editor Commands

This project uses UnityMCP for editor interaction. Use the MCP tools to:
- Build: `manage_build` tool
- Run tests: `run_tests` tool (Unity Test Framework 1.6.0 is installed)
- Play mode: `manage_editor` tool with `action="set_play_mode"`
- Check compilation errors: `read_console` after any script changes

After creating or modifying scripts, always check `read_console` for compilation errors before proceeding.

## Asset Organization

**All game content must live under `Assets/GameContent/`**, organized by type:
- `Assets/GameContent/Scripts/` — all C# game code
- `Assets/GameContent/Animations/` — animation clips and controllers
- `Assets/GameContent/Scenes/` — game scenes
- Other content types follow the same pattern

Do not place game assets directly in `Assets/` root or outside `GameContent/`.

## Code Conventions

- **Static methods must end with `ST`** (e.g., `GetInstanceST()`, `LoadDataST()`)
- Use OOP best practices; design systems with expansion and refactoring in mind
- Use the **Factory pattern** for save/load objects
- Input must be designed around **single-button** interaction: left mouse click on PC, touch on Android

## Save / Load

Use **Easy Save 3** (already installed at `Assets/Plugins/Easy Save 3/`) for all persistence. Data saves to the application's AppData directory.

## Rendering & UI

- Render pipeline: **Universal Render Pipeline (URP)** 17.3.0
- Input system: **New Input System** 1.18.0 (config at `Assets/InputSystem_Actions.inputactions`)
- All UI targets **portrait mode 1080×1920**; the game must stay in portrait mode on Android regardless of device rotation

## Key Packages

| Package | Version | Purpose |
|---|---|---|
| `com.unity.render-pipelines.universal` | 17.3.0 | URP rendering |
| `com.unity.inputsystem` | 1.18.0 | New Input System |
| `com.unity.2d.*` | various | 2D sprites, tilemaps, animation, Aseprite import |
| `com.unity.test-framework` | 1.6.0 | Unit/integration tests |
| `com.unity.timeline` | 1.8.10 | Cutscenes/sequences |
| Easy Save 3 (plugin) | — | Save/load to AppData |
| TextMesh Pro (built-in) | — | Text rendering |

## Gameplay Scope

The initial implementation mirrors Pokémon Red/Blue exactly (overworld exploration, turn-based battles, type system, etc.) before horror/roguelite elements are added. Keep systems modular so they can be extended without full rewrites.
