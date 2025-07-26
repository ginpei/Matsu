using System;
using System.Threading;

namespace ConsoleDemo
{
    class Program
    {
        private static bool _running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello from Sasuke Console Demo!");
            Console.WriteLine("========================================");

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
                
                if (int.TryParse(input, out int choice))
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
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("Hello from Sasuke Console Demo!");
            Console.WriteLine("========================================");
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. System Information");
            Console.WriteLine("2. Show Hello");
            Console.WriteLine("3. Show Yo");
            Console.WriteLine("0. Exit");
            Console.WriteLine("========================================");
            Console.Write("Enter your choice: ");
        }

        private static void ProcessMenuChoice(int choice)
        {
            Console.Clear();
            
            switch (choice)
            {
                case 1:
                    ShowSystemInformation();
                    break;
                case 2:
                    ShowHello();
                    break;
                case 3:
                    ShowYo();
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

        private static void ShowSystemInformation()
        {
            Console.WriteLine("System Information");
            Console.WriteLine("==================");
            Console.WriteLine($"Current Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Operating System: {Environment.OSVersion}");
            Console.WriteLine($".NET Framework Version: {Environment.Version}");
            Console.WriteLine($"Machine Name: {Environment.MachineName}");
            Console.WriteLine($"User Name: {Environment.UserName}");
        }

        private static void ShowHello()
        {
            Console.WriteLine("Hello");
        }

        private static void ShowYo()
        {
            Console.WriteLine("Yo");
        }
    }
}