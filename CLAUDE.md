# Game Jam Project - Developer Guide

Quick commands and rules for high-speed game development iterations.

## Build and Test Commands

### Running the Game / Build

- **Build standalone PC build**:

  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\<YOUR_VERSION>\Editor\Unity.exe" -batchmode -quit -projectPath . -buildWindows64Player Build/Windows/GameJamBuild.exe
  ```

---

## Code & Unity Conventions

### Core Rules

- **Coding Standard**: Keep scripts simple. PascalCase for public members, camelCase for local variables.
- **Cache References**: Always cache `GetComponent` and `Camera.main` in `Awake` / `Start`. Avoid calling them in loops or `Update`.
- **Optimization**: Avoid GC allocations in `Update()`. Reuse yield instructions or use a cache.
- ⚠️ **Meta Files**: Stage/delete `.meta` files together with their corresponding assets or scripts.

---

## Claude Subagents & Orchestration

The project is structured with specialized subagents located in `.claude/agents/` to handle specific tasks:

- `jam-core-architect`: Manages scene management, game loops, singleton managers, inputs, and base logic.
- `jam-gameplay-coder`: Manages gameplay features, characters, enemies, interactions, and level systems.
- `jam-audio-ui`: Manages canvas UI, panel controls, sound effect integrations, and audio events.

### Orchestration Guidelines

- **Core system changes**: Delegate changes in core managers or loaders to `jam-core-architect`.
- **Gameplay feature additions**: Delegate scripts for mechanics to `jam-gameplay-coder`.
- **UI & sound integration**: Delegate UI layout scripts and audio triggers to `jam-audio-ui`.

---

## Debugging Rules

- ALWAYS check Unity console log files or terminal output before declaring a bug fixed.
- NEVER run full rebuilds or asset re-imports unless necessary.
- KEEP prototyping code clean, but prioritize speed and functionality during the jam!
