using System;
using System.Threading;

namespace VolumeDemo
{
    class Program
    {
        private static bool _running = true;
        private static VolumeManager _volumeManager;

        static void Main(string[] args)
        {
            // Initialize volume manager
            _volumeManager = new VolumeManager();
            
            // Set up Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _running = false;
                Console.WriteLine("\nShutting down...");
            };

            // Interactive menu loop
            while (_running)
            {
                ShowMenu();
                string input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    // Empty input - just refresh menu silently
                    continue;
                }
                else if (int.TryParse(input, out int choice))
                {
                    ProcessMenuChoice(choice);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            
            // Clean up
            _volumeManager?.Dispose();
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Volume Demo");
            Console.WriteLine("============");
            
            if (_volumeManager.IsAvailable)
            {
                Console.WriteLine($"Device: {_volumeManager.DeviceName}");
                Console.WriteLine($"Current Volume: {_volumeManager.CurrentVolume}{(_volumeManager.IsMuted ? " (MUTED)" : "")}");
            }
            else
            {
                Console.WriteLine("No audio device available");
            }
            
            Console.WriteLine();
            Console.WriteLine("1. Set volume");
            Console.WriteLine("2. Toggle mute/unmute");
            Console.WriteLine("3. Real-time volume monitor");
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");
        }

        private static void ProcessMenuChoice(int choice)
        {
            Console.Clear();
            
            switch (choice)
            {
                case 1:
                    SetVolume();
                    break;
                case 2:
                    ToggleMute();
                    break;
                case 3:
                    RealTimeVolumeMonitor();
                    break;
                case 0:
                    _running = false;
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please select a valid option.");
                    break;
            }
            
            if (_running)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }


        private static void SetVolume()
        {
            if (!_volumeManager.IsAvailable)
            {
                Console.WriteLine("No audio device available.");
                return;
            }
            
            Console.Write("Enter volume level (0-100): ");
            string input = Console.ReadLine();
            
            if (int.TryParse(input, out int volume))
            {
                if (volume >= 0 && volume <= 100)
                {
                    if (_volumeManager.SetVolume(volume))
                    {
                        Console.WriteLine($"Volume set to {volume}%");
                    }
                    else
                    {
                        Console.WriteLine("Error setting volume.");
                    }
                }
                else
                {
                    Console.WriteLine("Volume must be between 0 and 100.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number between 0 and 100.");
            }
        }

        private static void ToggleMute()
        {
            if (!_volumeManager.IsAvailable)
            {
                Console.WriteLine("No audio device available.");
                return;
            }
            
            bool wasMuted = _volumeManager.IsMuted;
            
            if (_volumeManager.ToggleMute())
            {
                Console.WriteLine(wasMuted ? "Audio unmuted." : "Audio muted.");
            }
            else
            {
                Console.WriteLine("Error toggling mute.");
            }
        }

        private static void RealTimeVolumeMonitor()
        {
            Console.WriteLine("Real-time Volume Monitor");
            Console.WriteLine("========================");
            Console.WriteLine("Press Enter to return to menu...");
            Console.WriteLine();
            
            // Event handler for volume changes
            void OnStateChanged(VolumeState current, VolumeState previous)
            {
                Console.WriteLine($"[{current.Timestamp:HH:mm:ss}] {current.DeviceName}: {current.Volume}%{(current.IsMuted ? " (MUTED)" : "")}");
            }
            
            // Subscribe to events and start monitoring
            _volumeManager.StateChanged += OnStateChanged;
            _volumeManager.StartMonitoring();
            
            // Show initial state
            if (_volumeManager.IsAvailable)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_volumeManager.DeviceName}: {_volumeManager.CurrentVolume}%{(_volumeManager.IsMuted ? " (MUTED)" : "")}");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] No audio device available");
            }
            
            // Wait for Enter key specifically to exit
            ConsoleKeyInfo key;
            do {
                key = Console.ReadKey(true);
            } while (key.Key != ConsoleKey.Enter);
            
            // Clean up
            _volumeManager.StopMonitoring();
            _volumeManager.StateChanged -= OnStateChanged;
            
            Console.WriteLine("\nReturning to menu...");
        }
    }
}