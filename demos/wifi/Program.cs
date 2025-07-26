using System;
using System.Threading;

namespace WiFiDemo
{
    class Program
    {
        private static WiFiMonitor _wifiMonitor;
        private static bool _running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("WiFi Connection Monitor Demo");
            Console.WriteLine("Press Ctrl+C to exit");
            Console.WriteLine("========================================");

            // Set up Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _running = false;
                Console.WriteLine("\nShutting down...");
            };

            try
            {
                _wifiMonitor = new WiFiMonitor();
                _wifiMonitor.StatusChanged += OnWiFiStatusChanged;

                if (!_wifiMonitor.Initialize())
                {
                    Console.WriteLine("Failed to initialize WiFi monitor.");
                    Console.WriteLine("Common issues:");
                    Console.WriteLine("- Location permissions not granted (Windows 11 24H2+)");
                    Console.WriteLine("- WiFi adapter not available");
                    Console.WriteLine("- Insufficient privileges");
                    return;
                }

                Console.WriteLine("WiFi monitor initialized successfully.");
                Console.WriteLine("Monitoring WiFi connection changes...");

                // Keep the application running
                while (_running)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                _wifiMonitor?.Dispose();
                Console.WriteLine("WiFi monitor stopped.");
            }
        }

        private static void OnWiFiStatusChanged(object sender, WiFiStatusEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            if (e.IsConnected)
            {
                Console.WriteLine($"[{timestamp}] Connected to: {e.SSID}");
            }
            else
            {
                Console.WriteLine($"[{timestamp}] Disconnected");
            }
        }
    }
}