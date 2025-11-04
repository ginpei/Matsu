# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Matsu is a WPF system tray application with accompanying demo projects and comprehensive documentation. The main application is built on .NET 8.0 and runs primarily in the background with system tray functionality. The repository includes demonstration projects for console applications, volume control, and WiFi monitoring, along with extensive technical documentation.

## Architecture

### Main Application (Matsu)
- **WPF Application**: Uses Windows Presentation Foundation for the GUI (.NET 8.0)
- **System Tray Integration**: Utilizes Windows Forms NotifyIcon for system tray functionality
- **Shutdown Management**: Configured with `ShutdownMode.OnExplicitShutdown` to persist in system tray when main window is closed
- **Navigation System**: Frame-based navigation between Dashboard and About pages with custom navigation button selection tracking

### Demo Projects (.NET Framework 4.8)
- **Console Demo**: Interactive menu-driven console application
- **Volume Demo**: Audio control demonstration using AudioSwitcher.AudioApi.CoreAudio
- **WiFi Demo**: WiFi connection monitoring using ManagedNativeWifi

### Key Components

**Main Application Core:**
- `App.xaml.cs`: Main application class that handles system tray setup, context menu creation, window management, and WiFi monitoring initialization
- `MainWindow.xaml/cs`: Primary application window with frame-based navigation between pages and custom navigation button states
- `DashboardPage.xaml/cs`: Main functional page integrating volume control and WiFi status monitoring with real-time updates
- `AboutPage.xaml/cs`: Simple about page for application information
- `WiFiStatusMonitor.cs`: WiFi connection status monitoring with event-driven architecture using ManagedNativeWifi
- `VolumeManager.cs`: Audio device management and volume control using AudioSwitcher.AudioApi.CoreAudio

**System Tray Features:**
- Left-click: Shows main window
- Right-click: Context menu with Settings, About, and Exit options
- Window closing is intercepted to hide instead of exit

**Demo Projects:**
- `demos/console/Program.cs`: Interactive console application with menu system
- `demos/volume/Program.cs`: Volume control demonstration
- `demos/wifi/WiFiMonitor.cs`: WiFi connection monitoring with event-driven architecture

## Development Commands

### Main Application
```bash
# Build main application
dotnet build

# Run main application
dotnet run

# Build for release
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Demo Projects
```bash
# Build specific demo projects (demos are excluded from main solution)
dotnet build demos/console/ConsoleDemo.csproj
dotnet build demos/volume/VolumeDemo.csproj
dotnet build demos/wifi/WiFiDemo.csproj

# Run specific demo projects
dotnet run --project demos/console/ConsoleDemo.csproj
dotnet run --project demos/volume/VolumeDemo.csproj
dotnet run --project demos/wifi/WiFiDemo.csproj

# Build all demo projects at once
dotnet build demos/console/ConsoleDemo.csproj && dotnet build demos/volume/VolumeDemo.csproj && dotnet build demos/wifi/WiFiDemo.csproj
```

## Dependencies

### Main Application
- .NET 8.0 Windows target framework
- Windows Presentation Foundation (WPF)
- Windows Forms (for NotifyIcon)
- **AudioSwitcher.AudioApi.CoreAudio** (3.0.3): Audio device control and volume management
- **ManagedNativeWifi** (2.8.0): WiFi interface management and connection monitoring

### Demo Projects
- .NET Framework 4.8
- **AudioSwitcher.AudioApi.CoreAudio** (3.0.3): Audio device control
- **ManagedNativeWifi** (2.8.0): WiFi interface management

## Project Structure

```
Matsu/                          # Main WPF application (.NET 8.0)
├── App.xaml[.cs]              # Application entry point with system tray
├── MainWindow.xaml[.cs]       # Primary window with navigation
├── DashboardPage.xaml[.cs]    # Main functional page
├── AboutPage.xaml[.cs]        # About information page
├── WiFiStatusMonitor.cs       # WiFi monitoring component
├── VolumeManager.cs           # Audio volume management
├── demos/                     # Demonstration projects (.NET Framework 4.8)
│   ├── console/               # Interactive console application
│   ├── volume/                # Audio control demo
│   └── wifi/                  # WiFi monitoring demo
└── docs/                      # Technical documentation
    ├── dpi-awareness.md       # High DPI implementation guide
    ├── task-tray.md          # System tray development guide
    └── wifi.md               # WiFi API research and implementation
```

## Documentation

Comprehensive technical documentation is available in the `/docs` folder:
- **DPI Awareness**: Implementation of high-DPI support with per-monitor awareness
- **System Tray**: Detailed guide for Windows Forms NotifyIcon implementation
- **WiFi Integration**: Research on Windows WiFi APIs and notification systems

## Development Notes

- Main application targets .NET 8.0 Windows while demos use .NET Framework 4.8
- Demo projects are excluded from main solution build via `Directory.Build.props`
- System tray application pattern: main window hides to tray rather than closing
- Demo projects demonstrate common Windows system integration patterns
- WiFi monitoring requires location permissions on Windows 11 24H2+
- Audio and WiFi components are integrated directly into the main application (not just demos)
- Real-time status updates are handled through event-driven architecture with proper UI thread dispatching
- Navigation uses WPF Frame control with custom button selection state management
- Volume control includes slider interface with click-to-position behavior
- All monitoring components implement IDisposable for proper resource cleanup
- No formal test projects are configured in this solution
- when i ask to research, just research and report the result