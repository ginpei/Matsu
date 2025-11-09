using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Matsu.Lib.Wifi;
using System.Diagnostics;
using Matsu.Lib.Audio;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Matsu.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugPage : Page
    {
        readonly WifiStore wifiStore;
        readonly AudioStore audioStore;

        public DebugPage()
        {
            InitializeComponent();

            wifiStore = new WifiStore();
            WifiNetworksPanel.DataContext = wifiStore;

            audioStore = new AudioStore();
            AudioPanel.DataContext = audioStore;

            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                CurrentThemeText.Text = rootElement.RequestedTheme.ToString();
            }
        }

        private void SystemThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("System");
            // Set theme on the root content instead of Application.Current
            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Default;
                CurrentThemeText.Text = rootElement.RequestedTheme.ToString();
            }
        }

        private void LightThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Light");
            // Set theme on the root content instead of Application.Current
            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
                CurrentThemeText.Text = rootElement.RequestedTheme.ToString();
            }
        }

        private void DarkThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Dark");
            // Set theme on the root content instead of Application.Current
            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Dark;
                CurrentThemeText.Text = rootElement.RequestedTheme.ToString();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is not Slider slider || audioStore == null)
            {
                return;
            }

            uint newVolume = (uint)Math.Round(slider.Value);
            audioStore.SetVolume(newVolume);
        }

        private void MuteToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (audioStore == null)
            {
                return;
            }

            audioStore.ToggleMute();
        }
    }
}
