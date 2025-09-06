using System.Windows;
using Forms = System.Windows.Forms; // Alias to avoid ambiguity
using System.Drawing;
using System.Diagnostics;

namespace Matsu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _notifyIcon;
        private WiFiStatusMonitor? _wifiMonitor;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = SystemIcons.Application, // Default icon
                Visible = true,
                Text = "Matsu"
            };

            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, (s, args) => ShowMainWindow());
            contextMenu.Items.Add("About", null, (s, args) => ShowAbout());
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.MouseClick += NotifyIcon_MouseClick;

            // Initialize WiFi monitoring
            _wifiMonitor = new WiFiStatusMonitor();
            _wifiMonitor.StatusChanged += OnWiFiStatusChanged;
            if (!_wifiMonitor.Initialize())
            {
                Debug.WriteLine("Failed to initialize WiFi monitoring");
            }
        }

        private void NotifyIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ShowMainWindow();
            }
            // Right click handled by context menu
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Hide();
            }
        }

        private MainWindow ShowMainWindow()
        {
            MainWindow? mainWindow = null;
            if (Current.MainWindow is MainWindow mw)
            {
                mainWindow = mw;
            }
            else
            {
                mainWindow = new MainWindow();
                Current.MainWindow = mainWindow;
                mainWindow.Closing += MainWindow_Closing;
            }
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            return mainWindow;
        }

        private void OnWiFiStatusChanged(object? sender, WiFiStatusEventArgs e)
        {
            string message = e.IsConnected 
                ? $"WiFi Connected: {e.SSID}" 
                : "WiFi Disconnected";
            Debug.WriteLine(message);
        }

        private void ShowAbout()
        {
            // TODO: Implement
            System.Windows.MessageBox.Show("Matsu by Ginpei", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _wifiMonitor?.Dispose();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
