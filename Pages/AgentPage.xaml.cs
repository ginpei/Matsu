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
using CommunityToolkit.Mvvm.ComponentModel;
using Matsu.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Matsu.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AgentPage : Page
    {
        private readonly AgentService _agentService = AgentService.Instance;

        public AgentPage()
        {
            InitializeComponent();

            // Update the toggle button when the agent state changes
            _agentService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AgentService.IsAgentEnabled))
                {
                    UpdateToggleButton();
                }
            };

            // Initialize the toggle button state
            UpdateToggleButton();
        }

        private void UpdateToggleButton()
        {
            AgentToggleButton.IsChecked = _agentService.IsAgentEnabled;
            AgentToggleButton.Content = _agentService.IsAgentEnabled ? "Enabled" : "Enable";

            if (_agentService.IsAgentEnabled)
            {
                AgentToggleButton.IsEnabled = false;
            }
        }

        private void AgentToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _agentService.ToggleAgent();
        }
    }
}