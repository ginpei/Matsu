using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ManagedNativeWifi;
using Matsu.Lib.Wifi;
using System.Diagnostics;

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

        public string GreetingMessage { get; set; } = "Hello, WinUI 3!";

        public DebugPage()
        {
            InitializeComponent();

            wifiStore = new WifiStore();
            WifiNetworksPanel.DataContext = wifiStore;

            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                CurrentThemeText.Text = rootElement.RequestedTheme.ToString();
            }
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void SystemThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("System");
            GreetingMessage = "System";
            CurrentThemeText.Text = "Sys";
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
    }
}
