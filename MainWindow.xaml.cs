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

        public MainWindow()
        {
            InitializeComponent();
            InitializeVolumeManager();
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

        private void OnVolumeStateChanged(VolumeState current, VolumeState previous)
        {
            Dispatcher.Invoke(() => UpdateVolumeDisplay());
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
                SliderValueLabel.Text = $"{_volumeManager.CurrentVolume}%";
                
                EnableControls();
            }
            else
            {
                DeviceNameLabel.Text = "Audio Device: No device available";
                VolumeStatusLabel.Text = "Volume: --%";
                VolumeStatusLabel.Foreground = Brushes.Gray;
                SliderValueLabel.Text = "--%";
                DisableControls();
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
            if (_volumeManager?.IsAvailable != true) 
            {
                // Update label even when no device is available
                if (SliderValueLabel != null)
                    SliderValueLabel.Text = $"{(int)e.NewValue}%";
                return;
            }
            
            try
            {
                int volume = (int)e.NewValue;
                if (SliderValueLabel != null)
                    SliderValueLabel.Text = $"{volume}%";
                
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
            base.OnClosed(e);
        }
    }
}