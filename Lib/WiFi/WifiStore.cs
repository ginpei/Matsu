using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Matsu.Lib.WiFi
{
    public partial class WifiStore : ObservableObject
    {
        private NativeWifiPlayer? _wifiPlayer;
        private readonly SynchronizationContext? _synchronizationContext;

        [ObservableProperty]
        private string _wifiState = "Initializing...";

        [ObservableProperty]
        private string _ssid = "";

        [ObservableProperty]
        private string _errorMessage = "";

        public WifiStore()
        {
            _synchronizationContext = SynchronizationContext.Current;

            try
            {
                _wifiPlayer = new NativeWifiPlayer();
                _wifiState = "Pending...";
            }
            catch (UnauthorizedAccessException)
            {
                _wifiState = "Failed to start up";
                _errorMessage = "Permission required for WiFi.";
            }

            if (_wifiPlayer != null)
            { 
                _wifiPlayer.ConnectionChanged += WifiPlayer_ConnectionChanged;
            }

            UpdateWifiStatus();
        }

        private void WifiPlayer_ConnectionChanged(object? sender, ConnectionChangedEventArgs e)
        {
            Debug.WriteLine($"status: {e.ChangedState.ToString()}");
             
            // Marshal the call back to the UI thread to avoid cross-thread exceptions
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(_ => UpdateWifiStatus(), null);
            }
            else
            {
                // Fallback for non-UI scenarios
                Task.Run(() => UpdateWifiStatus());
            }
        }

        private void UpdateWifiStatus()
        {
            if (_wifiPlayer == null)
            {
                return;
            }

            var ssids = NativeWifi.EnumerateConnectedNetworkSsids();
            if (ssids.Count() > 0)
            {
                WifiState = "Connected";
                Ssid = ssids.First().ToString();
            }
            else
            {
                WifiState = "Not Connected";
                Ssid = "";
            }
        }
    }
}
