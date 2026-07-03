---
name: commit
description: Stage changes and create a git commit with a clear, concise description of additions.
user-invocable: true
argument-hint: "Short description of what you did"
---

Staging changed files and committing them cleanly for the Game Jam.

## Pre-commit Checklist

1. Check for compilation errors or unfinished debugging comments.
2. ⚠️ **Unity Meta Check**: Verify that every added, renamed, or deleted asset/script has its corresponding `.meta` file staged/deleted together.

## Steps

1. Run `git status` to see what changed.
2. Stage both files and their corresponding `.meta` files.
3. Draft a commit message:
   - Example: `feat(player): add dash ability`
   - Example: `fix(ui): fix gameover panel scaling`
4. Commit:
   ```bash
   git commit -m "$ARGUMENTS"
   ```
