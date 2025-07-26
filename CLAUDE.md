# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Matsu is a WPF application built on .NET 8.0 that features system tray functionality. The application runs primarily in the background with a system tray icon and context menu for user interaction.

## Architecture

- **WPF Application**: Uses Windows Presentation Foundation for the GUI
- **System Tray Integration**: Utilizes Windows Forms NotifyIcon for system tray functionality
- **Shutdown Management**: Configured with `ShutdownMode.OnExplicitShutdown` to persist in system tray when main window is closed

### Key Components

- `App.xaml.cs`: Main application class that handles system tray setup, context menu creation, and window management
- `MainWindow.xaml/cs`: Primary application window (currently minimal/empty)
- System tray features:
  - Left-click: Shows main window
  - Right-click: Context menu with Settings, About, and Exit options
  - Window closing is intercepted to hide instead of exit

## Development Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Build for Release
```bash
dotnet build -c Release
```

### Clean
```bash
dotnet clean
```

## Project Structure

- Single-project solution targeting `net8.0-windows`
- Uses both WPF (`UseWPF=true`) and Windows Forms (`UseWindowsForms=true`) for system tray functionality
- Nullable reference types enabled
- Implicit usings enabled

## Development Notes

- The application is designed to run in the system tray rather than as a traditional windowed application
- Main window closes to system tray rather than terminating the application
- Context menu provides primary user interaction points
- About dialog is implemented as a simple MessageBox (marked for future enhancement)