using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Matsu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Attached property to track button selection state
        public static readonly DependencyProperty IsSelectedNavButtonProperty =
            DependencyProperty.RegisterAttached("IsSelectedNavButton", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static void SetIsSelectedNavButton(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedNavButtonProperty, value);
        }

        public static bool GetIsSelectedNavButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedNavButtonProperty);
        }
        private DashboardPage? _dashboardPage;
        private AboutPage? _aboutPage;
        private System.Windows.Controls.Button? _selectedNavButton;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load Dashboard page by default and set as selected
            NavigateToPage("Dashboard");
            SetSelectedNavButton(DashboardNavButton);
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string pageName)
            {
                NavigateToPage(pageName);
                SetSelectedNavButton(button);
            }
        }

        private void SetSelectedNavButton(System.Windows.Controls.Button selectedButton)
        {
            var allNavButtons = new[] { DashboardNavButton, AboutNavButton };
            
            // Reset all buttons to unselected state
            foreach (var button in allNavButtons)
            {
                SetIsSelectedNavButton(button, false);
                button.Background = System.Windows.Media.Brushes.Transparent;
                
                // Reset to default colors
                var sp = button.Content as StackPanel;
                if (sp != null)
                {
                    bool isFirstChild = true;
                    foreach (var child in sp.Children)
                    {
                        if (child is TextBlock tb)
                        {
                            if (isFirstChild) // Icon (first TextBlock)
                            {
                                tb.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x78, 0xD4)); // Blue icon
                                isFirstChild = false;
                            }
                            else // Text label (second TextBlock)
                            {
                                tb.Foreground = System.Windows.Media.Brushes.Black;
                            }
                        }
                    }
                }
            }
            
            // Set the selected button state
            _selectedNavButton = selectedButton;
            SetIsSelectedNavButton(selectedButton, true);
            selectedButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x78, 0xD4)); // Windows accent blue
            
            // Update selected button colors
            var stackPanel = selectedButton.Content as StackPanel;
            if (stackPanel != null)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        textBlock.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
            }
        }

        private void NavigateToPage(string pageName)
        {
            Page? page = pageName switch
            {
                "Dashboard" => GetOrCreateDashboardPage(),
                "About" => GetOrCreateAboutPage(),
                _ => GetOrCreateDashboardPage()
            };

            if (page != null)
            {
                ContentFrame.Navigate(page);
            }
        }

        private DashboardPage GetOrCreateDashboardPage()
        {
            if (_dashboardPage == null)
            {
                _dashboardPage = new DashboardPage();
            }
            return _dashboardPage;
        }

        private AboutPage GetOrCreateAboutPage()
        {
            if (_aboutPage == null)
            {
                _aboutPage = new AboutPage();
            }
            return _aboutPage;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up dashboard page when window closes
            _dashboardPage?.Cleanup();
            base.OnClosed(e);
        }
    }
}