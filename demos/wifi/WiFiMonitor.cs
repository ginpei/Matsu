using System;
using System.Linq;
using ManagedNativeWifi;

namespace WiFiDemo
{
    public class WiFiMonitor : IDisposable
    {
        private bool _disposed = false;
        private NativeWifiPlayer _wifiPlayer;
        private bool? _lastConnectionState = null;
        private string _lastSSID = string.Empty;

        public event EventHandler<WiFiStatusEventArgs> StatusChanged;

        public WiFiMonitor()
        {
            _wifiPlayer = new NativeWifiPlayer();
            _wifiPlayer.ConnectionChanged += OnConnectionChanged;
        }

        public bool Initialize()
        {
            try
            {
                // Get initial status
                GetCurrentStatus();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Location permission required. Please enable in Windows Settings > Privacy & security > Location");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing WiFi monitor: {ex.Message}");
                return false;
            }
        }

        private void OnConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            Console.WriteLine($"[DEBUG] ConnectionChanged Event:");
            Console.WriteLine($"  Interface ID: {e.InterfaceId}");
            Console.WriteLine($"  Changed State: {e.ChangedState}");
            Console.WriteLine($"  Timestamp: {DateTime.Now:HH:mm:ss.fff}");
            
            if (e.Data != null)
            {
                Console.WriteLine($"  Connection Mode: {e.Data.ConnectionMode}");
                Console.WriteLine($"  Profile Name: {e.Data.ProfileName}");
                // Note: SSID property may not be available in ConnectionNotificationData
                // Console.WriteLine($"  SSID: {e.Data.Ssid}");
            }
            Console.WriteLine();
            
            GetCurrentStatus();
        }

        public void GetCurrentStatus()
        {
            try
            {
                // Use the interface connections enumeration method that I know exists
                var interfaceConnections = NativeWifi.EnumerateInterfaceConnections();
                
                Console.WriteLine($"[DEBUG] Found {interfaceConnections.Count()} interfaces:");
                foreach (var iface in interfaceConnections)
                {
                    Console.WriteLine($"  Interface: {iface.Id}");
                    Console.WriteLine($"  Description: {iface.Description}");
                    Console.WriteLine($"  Connected: {iface.IsConnected}");
                    Console.WriteLine($"  Profile: {iface.ProfileName}");
                }
                
                bool currentConnectionState = false;
                string currentSSID = string.Empty;
                
                foreach (var interfaceConnection in interfaceConnections)
                {
                    if (interfaceConnection.IsConnected)
                    {
                        currentConnectionState = true;
                        currentSSID = interfaceConnection.ProfileName; // Use profile name as a fallback
                        break;
                    }
                }
                
                Console.WriteLine($"[DEBUG] State Check:");
                Console.WriteLine($"  Previous: {_lastConnectionState?.ToString() ?? "unknown"}");
                Console.WriteLine($"  Current: {currentConnectionState}");
                Console.WriteLine($"  Previous SSID: '{_lastSSID}'");
                Console.WriteLine($"  Current SSID: '{currentSSID}'");
                Console.WriteLine($"  State Changed: {_lastConnectionState != currentConnectionState}");
                Console.WriteLine($"  SSID Changed: {_lastSSID != currentSSID}");
                Console.WriteLine();
                
                // Only fire event if state or SSID actually changed
                if (_lastConnectionState != currentConnectionState || _lastSSID != currentSSID)
                {
                    _lastConnectionState = currentConnectionState;
                    _lastSSID = currentSSID;
                    StatusChanged?.Invoke(this, new WiFiStatusEventArgs(currentConnectionState, currentSSID));
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Location permission required. Please enable in Windows Settings > Privacy & security > Location");
                StatusChanged?.Invoke(this, new WiFiStatusEventArgs(false, string.Empty));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current status: {ex.Message}");
                StatusChanged?.Invoke(this, new WiFiStatusEventArgs(false, string.Empty));
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _wifiPlayer?.Dispose();
                _disposed = true;
            }
        }
    }

    public class WiFiStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public string SSID { get; }

        public WiFiStatusEventArgs(bool isConnected, string ssid)
        {
            IsConnected = isConnected;
            SSID = ssid ?? string.Empty;
        }
    }
}