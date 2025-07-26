# Windows WiFi API Research

## Overview

This document provides research findings on Windows APIs for WiFi status change notifications and SSID retrieval, specifically for C# .NET Framework applications.

## WiFi Status Change Notifications

### Native WLAN API

The Windows Native WLAN API provides robust notifications for WiFi status changes through the `WlanRegisterNotification` function.

#### Key Components

1. **WlanRegisterNotification Function**
   - Primary method for registering WiFi status change notifications
   - Requires `WLAN_NOTIFICATION_SOURCE_ACM` as the notification source
   - Affected by WiFi access permissions in modern Windows versions

2. **Available Notification Types**
   - **Connection Events**: `wlan_notification_acm_connection_start`, `wlan_notification_acm_connection_complete`
   - **Disconnection Events**: `wlan_notification_acm_disconnecting`, `wlan_notification_acm_disconnected`
   - **Scan Events**: `wlan_notification_acm_scan_complete`, `wlan_notification_acm_scan_list_refresh`
   - **Profile Events**: `wlan_notification_acm_profile_change`
   - **Scan Failures**: `wlan_notification_acm_scan_fail`

#### Implementation Requirements

```csharp
// Basic P/Invoke declarations needed:
[DllImport("wlanapi.dll")]
static extern int WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, out uint pdwNegotiatedVersion, out IntPtr phClientHandle);

[DllImport("wlanapi.dll")]
static extern int WlanRegisterNotification(IntPtr hClientHandle, uint dwNotifSource, bool bIgnoreDuplicate, WlanNotificationCallback funcCallback, IntPtr pCallbackContext, IntPtr pReserved, out uint pdwPrevNotifSource);

[DllImport("wlanapi.dll")]
static extern int WlanCloseHandle(IntPtr hClientHandle, IntPtr pReserved);

[DllImport("wlanapi.dll")]
static extern void WlanFreeMemory(IntPtr pMemory);

// Callback delegate
delegate void WlanNotificationCallback(ref WLAN_NOTIFICATION_DATA notificationData, IntPtr context);
```

#### Privacy and Permission Considerations

**Windows 11 24H2+ Requirements:**
- On Windows 11 (24H2) or newer, methods require user's permission to access location information
- Without permission, `UnauthorizedAccessException` will be thrown
- Permission can be set in: **Settings > Privacy & security > Location**

**General Windows Considerations:**
- Modern Windows versions require proper permission handling for location and WiFi access
- Apps should call `WiFiAdapter.RequestAccessAsync` to trigger system permission prompt (UWP/WinRT)
- Subscribe to `AppCapability.AccessChanged` event to respond to permission changes
- The `wiFiControl` device capability may be required for certain notification types

**ManagedNativeWifi Permission Handling:**
```csharp
try
{
    var interfaces = NativeWifi.EnumerateInterfaces();
    // Process interfaces...
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Location permission required. Please enable in Windows Settings > Privacy & security > Location");
}
```

## SSID Retrieval

### Getting Current Connected SSID

To retrieve the SSID of the currently connected WiFi network, use `WlanQueryInterface` with `wlan_intf_opcode_current_connection`.

#### Implementation Steps

1. **Open WLAN Handle**
   ```csharp
   WlanOpenHandle(2, 0, out var _, out var clientHandle);
   ```

2. **Query Current Connection**
   ```csharp
   WlanQueryInterface(
       clientHandle, 
       interfaceGuid, 
       wlan_intf_opcode_current_connection, 
       0, 
       out var _, 
       out var data, 
       out var _);
   ```

3. **Extract SSID**
   ```csharp
   var connectionAttributes = Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(data);
   var ssid = connectionAttributes.wlanAssociationAttributes.dot11Ssid.ucSSID;
   ```

4. **Clean Up**
   ```csharp
   WlanFreeMemory(data);
   WlanCloseHandle(clientHandle);
   ```

#### SSID Structure Details

- SSID is defined as an octet string (byte array) with 0-32 length per IEEE 802.11 specifications
- Can be UTF-8 encoded string
- The `DOT11_SSID` structure provides methods to convert to both byte array and UTF-8 string formats

### Available Networks

Use `WlanGetAvailableNetworkList` to retrieve all available WiFi networks in range.

```csharp
[DllImport("wlanapi.dll")]
static extern int WlanGetAvailableNetworkList(
    IntPtr hClientHandle,
    ref Guid pInterfaceGuid,
    uint dwFlags,
    IntPtr pReserved,
    out IntPtr ppAvailableNetworkList);
```

## NuGet Package Solutions (Recommended)

### ManagedNativeWifi (Primary Recommendation)

**ManagedNativeWifi** is a modern, well-maintained managed implementation of the Native WiFi API that significantly simplifies WiFi management in .NET applications.

#### Installation
```bash
Install-Package ManagedNativeWifi
# or
dotnet add package ManagedNativeWifi
```

**Package Reference (csproj):**
```xml
<PackageReference Include="ManagedNativeWifi" Version="2.8.0" />
```

#### Key Features
- ✅ **No P/Invoke required** - Clean managed API
- ✅ **Event-driven monitoring** - Simple connection change notifications  
- ✅ **Proper error handling** - Built-in exception management
- ✅ **Memory management** - Automatic cleanup
- ✅ **Cross-platform** - .NET Standard 2.0 (compatible with .NET Framework 4.8+)
- ✅ **Active maintenance** - Regular updates and bug fixes

#### API Examples

**1. Connection Event Monitoring**
```csharp
using ManagedNativeWifi;

public class WiFiMonitor : IDisposable
{
    public event EventHandler<WiFiStatusChangedEventArgs> StatusChanged;

    public WiFiMonitor()
    {
        NativeWifi.ConnectionChanged += OnConnectionChanged;
    }

    private void OnConnectionChanged(object sender, EventArgs e)
    {
        CheckCurrentConnection();
    }

    public void CheckCurrentConnection()
    {
        foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
        {
            if (interfaceInfo.State == InterfaceState.Connected)
            {
                var (result, connection) = NativeWifi.GetCurrentConnection(interfaceInfo.Id);
                if (result == ActionResult.Success)
                {
                    StatusChanged?.Invoke(this, new WiFiStatusChangedEventArgs(true, connection.Ssid.ToString()));
                    return;
                }
            }
        }
        StatusChanged?.Invoke(this, new WiFiStatusChangedEventArgs(false, string.Empty));
    }

    public void Dispose()
    {
        NativeWifi.ConnectionChanged -= OnConnectionChanged;
    }
}
```

**2. Available Networks Enumeration**
```csharp
public static void ListAvailableNetworks()
{
    foreach (var network in NativeWifi.EnumerateAvailableNetworks())
    {
        Console.WriteLine($"SSID: {network.Ssid}");
        Console.WriteLine($"Signal: {network.SignalQuality}%");
        Console.WriteLine($"Security: {network.SecurityEnabled}");
        Console.WriteLine("---");
    }
}
```

**3. Interface State Monitoring**
```csharp
public static void MonitorInterfaceStates()
{
    foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
    {
        Console.WriteLine($"Interface: {interfaceInfo.Description}");
        Console.WriteLine($"State: {interfaceInfo.State}");
        
        switch (interfaceInfo.State)
        {
            case InterfaceState.Connected:
                var (result, connection) = NativeWifi.GetCurrentConnection(interfaceInfo.Id);
                if (result == ActionResult.Success)
                {
                    Console.WriteLine($"Connected to: {connection.Ssid}");
                    Console.WriteLine($"Profile: {connection.ProfileName}");
                    Console.WriteLine($"Authentication: {connection.AuthenticationAlgorithm}");
                }
                break;
            case InterfaceState.Disconnected:
                Console.WriteLine("Not connected");
                break;
            case InterfaceState.Authenticating:
                Console.WriteLine("Authenticating...");
                break;
        }
    }
}
```

### Alternative NuGet Packages

1. **SimpleWifi**
   - Stable reimplementation fixing older ManagedWifi issues
   - Easier to use than raw P/Invoke
   - Available on NuGet but less actively maintained

2. **Legacy ManagedWifi**
   - Older implementation with known stability issues
   - Use ManagedNativeWifi instead for new projects

## Implementation Approach Comparison

### ManagedNativeWifi vs Raw P/Invoke

| Aspect | ManagedNativeWifi | Raw P/Invoke |
|--------|-------------------|--------------|
| **Complexity** | ✅ Simple API calls | ❌ Complex P/Invoke declarations |
| **Error Handling** | ✅ Built-in exception handling | ❌ Manual error code checking |
| **Memory Management** | ✅ Automatic cleanup | ❌ Manual `WlanFreeMemory` calls |
| **Type Safety** | ✅ Strongly typed | ❌ IntPtr and marshalling |
| **Event Handling** | ✅ Clean .NET events | ❌ Callback delegates |
| **Code Maintenance** | ✅ High-level abstractions | ❌ Low-level implementation details |
| **Dependencies** | ❌ NuGet package dependency | ✅ No external dependencies |
| **Performance** | ✅ Optimized implementation | ⚠️ Depends on implementation quality |
| **Learning Curve** | ✅ Standard .NET patterns | ❌ Windows API knowledge required |

### Recommendation

- **Use ManagedNativeWifi** for most applications - it provides a robust, tested solution with clean APIs
- **Use Raw P/Invoke** only when you need maximum control, want to avoid dependencies, or have specific requirements not covered by the NuGet package

## Raw P/Invoke Implementation (Advanced)

For scenarios requiring maximum control or avoiding external dependencies, the native WLAN API can be used directly through P/Invoke declarations.

### Modern Windows APIs

- **Windows.Devices.WiFi** namespace (UWP/WinRT)
- **NetworkInformation** classes
- **Geolocator** API (requires location permissions)

## Memory Management

- Always call `WlanFreeMemory` after using functions that allocate memory
- Properly dispose of handles using `WlanCloseHandle`
- Use `using` statements or try-finally blocks for cleanup

## Important Notes

- API behavior changes are planned for fall 2024
- .NET Framework doesn't have built-in WiFi management APIs
- Native WiFi API requires P/Invoke declarations
- Requires proper error handling for various failure scenarios
- Not possible to get SSIDs on devices without WiFi capability

## Sample Code Structure

```csharp
public class WiFiManager : IDisposable
{
    private IntPtr _clientHandle;
    private bool _disposed = false;

    public WiFiManager()
    {
        // Initialize WLAN handle
        WlanOpenHandle(2, 0, out var _, out _clientHandle);
        
        // Register for notifications
        WlanRegisterNotification(_clientHandle, 
            WLAN_NOTIFICATION_SOURCE_ACM, 
            false, 
            NotificationCallback, 
            IntPtr.Zero, 
            IntPtr.Zero, 
            out var _);
    }

    private void NotificationCallback(ref WLAN_NOTIFICATION_DATA data, IntPtr context)
    {
        // Handle different notification types
        switch (data.NotificationCode)
        {
            case WLAN_NOTIFICATION_ACM_CONNECTION_COMPLETE:
                // Handle connection complete
                break;
            case WLAN_NOTIFICATION_ACM_DISCONNECTED:
                // Handle disconnection
                break;
            // Add other cases as needed
        }
    }

    public string GetCurrentSSID()
    {
        // Implementation to get current SSID
        // Return UTF-8 decoded SSID string
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            WlanCloseHandle(_clientHandle, IntPtr.Zero);
            _disposed = true;
        }
    }
}
```

## References

- [Native Wifi API Documentation](https://learn.microsoft.com/en-us/windows/win32/api/_nwifi/)
- [WlanRegisterNotification Function](https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlanregisternotification)
- [WlanGetAvailableNetworkList Function](https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/nf-wlanapi-wlangetavailablenetworklist)
- [WiFi Access and Location Changes](https://learn.microsoft.com/en-us/windows/win32/nativewifi/wi-fi-access-location-changes)