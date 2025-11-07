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

        private string _wifiState = "Initializing...";
        public string WifiState
        {
            get => _wifiState;
            set => SetProperty(ref _wifiState, value);
        }

        private string _ssid = "";
        public string Ssid
        {
            get => _ssid;
            set => SetProperty(ref _ssid, value);
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public WifiStore()
        {
            _synchronizationContext = SynchronizationContext.Current;

            try
            {
                _wifiPlayer = new NativeWifiPlayer();
                WifiState = "Pending...";
            }
            catch (UnauthorizedAccessException)
            {
                WifiState = "Failed to start up";
                ErrorMessage = "Permission required for WiFi.";
            }

            if (_wifiPlayer != null)
            { 
                _wifiPlayer.ConnectionChanged += WifiPlayer_ConnectionChanged;
            }

            UpdateWifiStatus(false);
        }

        private void WifiPlayer_ConnectionChanged(object? sender, ConnectionChangedEventArgs e)
        {
            Debug.WriteLine($"status: {e.ChangedState.ToString()}");

            var pending = e.ChangedState != ConnectionChangedState.Completed && e.ChangedState != ConnectionChangedState.Disconnected && e.ChangedState != ConnectionChangedState.Failed;

            // Marshal the call back to the UI thread to avoid cross-thread exceptions
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(_ => UpdateWifiStatus(pending), null);
            }
            else
            {
                // Fallback for non-UI scenarios
                Task.Run(() => UpdateWifiStatus(pending));
            }
        }

        private void UpdateWifiStatus(bool pending)
        {
            if (_wifiPlayer == null)
            {
                return;
            }


            if (pending)
            {
                WifiState = "Pending...";
                Ssid = "";
            }
            else
            {
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
}
