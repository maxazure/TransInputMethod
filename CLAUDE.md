# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a floating translation input method tool for Windows, built with .NET 8 WinForms. The application provides:
- Global hotkey-triggered floating translation window
- Chinese ⇄ English translation using LLM APIs
- Local SQLite database for translation history
- Borderless floating UI that follows cursor position
- Separate history viewing window with pagination

## Key Architecture Components

### Core Windows Forms
- **FloatingTranslationForm**: Borderless (`FormBorderStyle=None`), topmost window with auto-height adjustment
- **HistoryForm**: Standard dialog with title bar for viewing translation history
- **SettingsForm**: Configuration window for API keys, hotkeys, and translation prompts

### Data Layer
- SQLite database (`translations.db`) with `history` table storing source_text, translated_text, timestamp
- Configuration stored in encrypted `config.json` using AES encryption
- Database operations use `Microsoft.Data.Sqlite`

### Global Hotkeys System
- Uses `SetWindowsHookEx` Windows API for system-wide hotkey monitoring
- Default hotkeys: `Shift+Enter` (show floating window), `Ctrl+Enter` (translate/copy)
- Customizable hotkey configuration in settings

### Translation Integration
- HTTP client calls to LLM REST APIs (GPT-4, etc.)
- Multiple translation scenarios with custom prompts ("生活", "书面", "科技")
- Language auto-detection by LLM model

## Development Commands

Since this is a .NET 8 project, use these commands via WSL calling Windows cmd.exe:

```bash
# Build the project
cmd.exe /c "dotnet build"

# Run the application
cmd.exe /c "dotnet run"

# Run tests
cmd.exe /c "dotnet test"

# Publish release
cmd.exe /c "dotnet publish -c Release"
```

## Database Schema

```sql
CREATE TABLE history (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  source_text TEXT NOT NULL,
  translated_text TEXT NOT NULL,
  timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

Use `sqlite` command to view local database:
```bash
sqlite translations.db
```

## UI Specifications

- Floating window: 600px fixed width, auto-height (max screen height)
- Position: Near mouse cursor/text cursor, avoiding input area occlusion
- Theme support: Light/dark mode with transparency options
- Controls: History button (⌚), navigation arrows (↑↓), scenario dropdown