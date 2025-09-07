using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace Matsu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VolumeManager? _volumeManager;
        private WiFiStatusMonitor? _wifiMonitor;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVolumeManager();
            InitializeWiFiMonitor();
        }

        private void InitializeVolumeManager()
        {
            try
            {
                _volumeManager = new VolumeManager();
                _volumeManager.StateChanged += OnVolumeStateChanged;
                _volumeManager.StartMonitoring();
                
                UpdateVolumeDisplay();
                SetStatus("Volume control initialized", Brushes.Green);
            }
            catch (Exception ex)
            {
                // Clean up partial initialization
                _volumeManager?.Dispose();
                _volumeManager = null;
                
                SetStatus($"Error initializing volume control: {ex.Message}", Brushes.Red);
                DisableControls();
            }
        }

        private void InitializeWiFiMonitor()
        {
            try
            {
                _wifiMonitor = new WiFiStatusMonitor();
                _wifiMonitor.StatusChanged += OnWiFiStatusChanged;
                
                if (_wifiMonitor.Initialize())
                {
                    UpdateWiFiDisplay();
                    SetStatus("System monitoring initialized", Brushes.Green);
                }
                else
                {
                    WiFiStatusLabel.Text = "WiFi: Permission required";
                    NetworkNameLabel.Text = "Enable location access in Windows Settings";
                    NetworkNameLabel.Foreground = Brushes.Orange;
                }
            }
            catch (Exception)
            {
                // Clean up partial initialization
                _wifiMonitor?.Dispose();
                _wifiMonitor = null;
                
                WiFiStatusLabel.Text = "WiFi: Error";
                NetworkNameLabel.Text = "WiFi monitoring unavailable";
                NetworkNameLabel.Foreground = Brushes.Red;
            }
        }

        private void OnVolumeStateChanged(VolumeState current, VolumeState previous)
        {
            Dispatcher.Invoke(() => UpdateVolumeDisplay());
        }

        private void OnWiFiStatusChanged(object? sender, WiFiStatusEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateWiFiDisplay());
        }

        private void UpdateVolumeDisplay()
        {
            if (_volumeManager?.IsAvailable == true)
            {
                DeviceNameLabel.Text = $"Audio Device: {_volumeManager.DeviceName}";
                VolumeStatusLabel.Text = $"Volume: {_volumeManager.CurrentVolume}%{(_volumeManager.IsMuted ? " (MUTED)" : "")}";
                VolumeStatusLabel.Foreground = _volumeManager.IsMuted ? Brushes.Red : Brushes.Black;
                
                // Update slider without triggering events
                VolumeSlider.ValueChanged -= VolumeSlider_ValueChanged;
                VolumeSlider.Value = _volumeManager.CurrentVolume;
                VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
                
                EnableControls();
            }
            else
            {
                DeviceNameLabel.Text = "Audio Device: No device available";
                VolumeStatusLabel.Text = "Volume: --%";
                VolumeStatusLabel.Foreground = Brushes.Gray;
                DisableControls();
            }
        }

        private void UpdateWiFiDisplay()
        {
            if (_wifiMonitor?.IsConnected == true)
            {
                WiFiStatusLabel.Text = "WiFi: Connected";
                WiFiStatusLabel.Foreground = Brushes.Green;
                NetworkNameLabel.Text = $"Network: {_wifiMonitor.CurrentSSID}";
                NetworkNameLabel.Foreground = Brushes.DarkBlue;
            }
            else
            {
                WiFiStatusLabel.Text = "WiFi: Disconnected";
                WiFiStatusLabel.Foreground = Brushes.Red;
                NetworkNameLabel.Text = "No network connection";
                NetworkNameLabel.Foreground = Brushes.Gray;
            }
        }

        private void EnableControls()
        {
            MuteToggleButton.IsEnabled = true;
            VolumeSlider.IsEnabled = true;
        }

        private void DisableControls()
        {
            MuteToggleButton.IsEnabled = false;
            VolumeSlider.IsEnabled = false;
        }

        private void SetStatus(string message, System.Windows.Media.Brush color)
        {
            StatusLabel.Text = message;
            StatusLabel.Foreground = color;
        }

        private void MuteToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_volumeManager?.ToggleMute() == true)
                {
                    bool isMuted = _volumeManager.IsMuted;
                    SetStatus(isMuted ? "Audio muted" : "Audio unmuted", Brushes.Green);
                }
                else
                {
                    SetStatus("Failed to toggle mute", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error toggling mute: {ex.Message}", Brushes.Red);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int volume = (int)e.NewValue;
                
                if (_volumeManager.SetVolume(volume))
                {
                    SetStatus($"Volume set to {volume}%", Brushes.Green);
                }
                else
                {
                    SetStatus("Failed to set volume", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error setting volume: {ex.Message}", Brushes.Red);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _volumeManager?.Dispose();
            _wifiMonitor?.Dispose();
            base.OnClosed(e);
        }
    }
}