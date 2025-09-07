using System;
using System.Threading;
using AudioSwitcher.AudioApi.CoreAudio;

namespace Matsu
{
    public class VolumeState
    {
        public int Volume { get; set; }
        public bool IsMuted { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public bool IsDeviceAvailable { get; set; }
        public DateTime Timestamp { get; set; }

        public VolumeState()
        {
            Timestamp = DateTime.Now;
        }

        public VolumeState(int volume, bool isMuted, string deviceName, bool isDeviceAvailable)
        {
            Volume = volume;
            IsMuted = isMuted;
            DeviceName = deviceName ?? "Unknown";
            IsDeviceAvailable = isDeviceAvailable;
            Timestamp = DateTime.Now;
        }
    }

    public class VolumeManager : IDisposable
    {
        private CoreAudioController? _audioController;
        private CoreAudioDevice? _defaultDevice;
        private System.Threading.Timer? _monitoringTimer;
        private VolumeState? _previousState;
        private bool _isMonitoring;
        private readonly object _lock = new object();

        public event Action<VolumeState, VolumeState>? StateChanged;

        public int CurrentVolume
        {
            get
            {
                lock (_lock)
                {
                    return _defaultDevice != null ? (int)_defaultDevice.Volume : 0;
                }
            }
        }

        public bool IsMuted
        {
            get
            {
                lock (_lock)
                {
                    return _defaultDevice?.IsMuted ?? false;
                }
            }
        }

        public string DeviceName
        {
            get
            {
                lock (_lock)
                {
                    return _defaultDevice?.FullName ?? "No device";
                }
            }
        }

        public bool IsAvailable
        {
            get
            {
                lock (_lock)
                {
                    return _defaultDevice != null;
                }
            }
        }

        public VolumeManager()
        {
            InitializeAudio();
            _previousState = GetCurrentState();
        }

        private void InitializeAudio()
        {
            try
            {
                _audioController = new CoreAudioController();
                RefreshDevice();
            }
            catch
            {
                _defaultDevice = null;
            }
        }

        private void RefreshDevice()
        {
            try
            {
                lock (_lock)
                {
                    _defaultDevice = _audioController?.DefaultPlaybackDevice;
                }
            }
            catch
            {
                lock (_lock)
                {
                    _defaultDevice = null;
                }
            }
        }

        private VolumeState GetCurrentState()
        {
            lock (_lock)
            {
                if (_defaultDevice == null)
                {
                    return new VolumeState(0, false, "No device", false);
                }

                try
                {
                    return new VolumeState(
                        (int)_defaultDevice.Volume,
                        _defaultDevice.IsMuted,
                        _defaultDevice.FullName,
                        true
                    );
                }
                catch
                {
                    return new VolumeState(0, false, "Error reading device", false);
                }
            }
        }

        public bool SetVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100");

            lock (_lock)
            {
                RefreshDevice();
                
                if (_defaultDevice == null)
                    return false;

                try
                {
                    _defaultDevice.Volume = volume;
                    CheckForStateChange();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool ToggleMute()
        {
            lock (_lock)
            {
                RefreshDevice();
                
                if (_defaultDevice == null)
                    return false;

                try
                {
                    bool wasMuted = _defaultDevice.IsMuted;
                    _defaultDevice.Mute(!wasMuted);
                    CheckForStateChange();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            _monitoringTimer = new System.Threading.Timer(MonitoringCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
        }

        private void MonitoringCallback(object? state)
        {
            if (!_isMonitoring)
                return;

            CheckForStateChange();
        }

        private void CheckForStateChange()
        {
            var currentState = GetCurrentState();
            
            if (HasStateChanged(_previousState, currentState))
            {
                var previous = _previousState;
                _previousState = currentState;
                
                StateChanged?.Invoke(currentState, previous);
            }
        }

        private bool HasStateChanged(VolumeState previous, VolumeState current)
        {
            if (previous == null || current == null)
                return true;

            return previous.Volume != current.Volume ||
                   previous.IsMuted != current.IsMuted ||
                   previous.DeviceName != current.DeviceName ||
                   previous.IsDeviceAvailable != current.IsDeviceAvailable;
        }

        public void Dispose()
        {
            StopMonitoring();
            _audioController?.Dispose();
        }
    }
}