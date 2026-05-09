# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build        # Build the project
dotnet run          # Build and launch the game window
```

## Architecture

The entire application lives in a single file, `Program.cs`, using Avalonia 11 on .NET 9.

**Three classes:**
- `App` — Avalonia application entry point; applies the Fluent theme and creates the window.
- `GameWindow : Window` — Hosts the board and a status label. Subscribes to `BoardControl.StateChanged` to refresh the turn/win/draw label.
- `BoardControl : Control` — Custom-drawn control that owns all game state (`_cells`, `Current`, `GameOver`, `WinLine`). Handles click-to-place via `OnPointerPressed`, win detection via `CheckWinner`, and rendering via `Render`.

**Key detail:** `BoardControl` fills its background in `Render` with a solid rectangle — this is required for Avalonia hit-testing to register pointer events on a custom `Control` subclass.
