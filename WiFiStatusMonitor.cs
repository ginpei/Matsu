using System;
using System.Linq;
using ManagedNativeWifi;

namespace Matsu
{
    public class WiFiStatusMonitor : IDisposable
    {
        private bool _disposed = false;
        private NativeWifiPlayer? _wifiPlayer;
        private bool? _lastConnectionState = null;
        private string _lastSSID = string.Empty;

        public event EventHandler<WiFiStatusEventArgs>? StatusChanged;

        public bool IsConnected => _lastConnectionState ?? false;
        public string CurrentSSID => _lastSSID;

        public WiFiStatusMonitor()
        {
            _wifiPlayer = new NativeWifiPlayer();
            _wifiPlayer.ConnectionChanged += OnConnectionChanged;
        }

        public bool Initialize()
        {
            try
            {
                GetCurrentStatus();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("WiFi monitoring requires location permission. Please enable in Windows Settings > Privacy & security > Location");
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing WiFi monitor: {ex.Message}");
                return false;
            }
        }

        private void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
        {
            GetCurrentStatus();
        }

        public void GetCurrentStatus()
        {
            try
            {
                var interfaceConnections = NativeWifi.EnumerateInterfaceConnections();
                
                bool currentConnectionState = false;
                string currentSSID = string.Empty;
                
                foreach (var interfaceConnection in interfaceConnections)
                {
                    if (interfaceConnection.IsConnected)
                    {
                        currentConnectionState = true;
                        currentSSID = interfaceConnection.ProfileName;
                        break;
                    }
                }
                
                if (_lastConnectionState != currentConnectionState || _lastSSID != currentSSID)
                {
                    _lastConnectionState = currentConnectionState;
                    _lastSSID = currentSSID;
                    StatusChanged?.Invoke(this, new WiFiStatusEventArgs(currentConnectionState, currentSSID));
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("WiFi monitoring requires location permission. Please enable in Windows Settings > Privacy & security > Location");
                StatusChanged?.Invoke(this, new WiFiStatusEventArgs(false, string.Empty));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting WiFi status: {ex.Message}");
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