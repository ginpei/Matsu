using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Matsu.Services
{
    /// <summary>
    /// Singleton service for managing the PrototypeAgent instance across the application
    /// </summary>
    public sealed class AgentService : ObservableObject
    {
        private static AgentService? _instance;
        private static readonly object _lock = new object();

        private Lib.Agent.PrototypeAgent? _agent;
        private bool _isAgentEnabled;

        /// <summary>
        /// Gets the singleton instance of AgentService
        /// </summary>
        public static AgentService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AgentService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the agent is currently enabled
        /// </summary>
        public bool IsAgentEnabled
        {
            get => _isAgentEnabled;
            private set => SetProperty(ref _isAgentEnabled, value);
        }

        private AgentService()
        {
            // Private constructor for singleton pattern
        }

        /// <summary>
        /// Enables the agent by creating a new instance
        /// </summary>
        public void EnableAgent()
        {
            if (_agent == null)
            {
                _agent = new Lib.Agent.PrototypeAgent();
                IsAgentEnabled = true;
            }
        }

        /// <summary>
        /// Disables the agent by disposing the current instance
        /// </summary>
        public void DisableAgent()
        {
            if (_agent != null)
            {
                // TODO _agent.Dispose();
                _agent = null;
                IsAgentEnabled = false;
            }
        }

        /// <summary>
        /// Toggles the agent state
        /// </summary>
        public void ToggleAgent()
        {
            if (IsAgentEnabled)
            {
                DisableAgent();
            }
            else
            {
                EnableAgent();
            }
        }
    }
}