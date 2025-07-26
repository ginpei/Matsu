# High DPI Support Implementation

## Problem

Windows Forms applications on high-DPI displays (like devices with 2x device pixel ratio) appear blurry by default. This occurs because Windows applies compatibility scaling to applications that don't declare DPI awareness, resulting in a blurred upscaled appearance.

## Solution Overview

Implemented per-monitor DPI awareness to ensure the Sasuke application renders crisp and properly scaled on high-DPI displays while maintaining compatibility with standard DPI screens.

## Implementation Details

### 1. Application Manifest (`app.manifest`)

Created a new manifest file with DPI awareness declarations:

```xml
<application xmlns="urn:schemas-microsoft-com:asm.v3">
  <windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </windowsSettings>
</application>
```

**Key Features:**
- `dpiAware`: Tells Windows the application handles DPI scaling
- `dpiAwareness`: Enables per-monitor DPI awareness (Windows 10 1607+)
- `PerMonitorV2`: Most advanced DPI awareness mode with dynamic scaling support

### 2. Application Configuration (`App.config`)

Added DPI awareness configuration for .NET Framework:

```xml
<System.Windows.Forms.ApplicationConfigurationSection>
  <add key="DpiAwareness" value="PerMonitorV2" />
</System.Windows.Forms.ApplicationConfigurationSection>
```

This setting works in conjunction with the manifest to ensure proper DPI handling in Windows Forms.

### 3. Project Configuration (`Sasuke.csproj`)

Linked the manifest file to the project:

```xml
<PropertyGroup>
  <ApplicationManifest>app.manifest</ApplicationManifest>
</PropertyGroup>
```

This embeds the manifest directly into the executable.

### 4. Runtime DPI Awareness (`Program.cs`)

Added programmatic DPI awareness setup:

```csharp
[DllImport("shcore.dll")]
private static extern int SetProcessDpiAwareness(int awareness);

private static void SetProcessDpiAwareness()
{
    try
    {
        SetProcessDpiAwareness(2); // PROCESS_PER_MONITOR_DPI_AWARE
    }
    catch
    {
        // Fallback for older Windows versions
    }
}
```

Called before `Application.EnableVisualStyles()` for proper initialization order.

## DPI Awareness Modes

| Mode | Value | Description |
|------|-------|-------------|
| `DPI_UNAWARE` | 0 | Application is not DPI aware (default, causes blur) |
| `DPI_AWARE` | 1 | System DPI aware (scales with primary monitor) |
| `PROCESS_PER_MONITOR_DPI_AWARE` | 2 | Per-monitor DPI aware (recommended) |

## Benefits

1. **Crisp Rendering**: Text and UI elements render at native resolution
2. **Multi-Monitor Support**: Each monitor's DPI is handled independently
3. **Dynamic Scaling**: Application responds to DPI changes at runtime
4. **Backward Compatibility**: Works on Windows 7+ with graceful fallback

## Testing

Test the application on:
- High-DPI displays (150%, 200%, 300% scaling)
- Multi-monitor setups with different DPI settings
- Standard DPI displays (100% scaling)

## Troubleshooting

### Common Issues:
- **Oversized Controls**: Check if custom drawing respects DPI scaling
- **Mixed DPI Scenarios**: Ensure forms handle DPI changes when moved between monitors
- **Third-party Controls**: Verify compatibility with DPI-aware applications

### Debugging:
- Use `Graphics.DpiX` and `Graphics.DpiY` to check current DPI
- Monitor Windows messages like `WM_DPICHANGED` for dynamic scaling events
- Test with different Windows display scaling settings

## References

- [Microsoft Learn: High DPI Support in Windows Forms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms)
- [Windows DPI Awareness API](https://docs.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-setprocessdpiawareness)
- [Application Manifest Schema](https://docs.microsoft.com/en-us/windows/win32/sbscs/application-manifests)