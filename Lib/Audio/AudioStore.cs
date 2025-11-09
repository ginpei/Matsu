using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Matsu.Lib.Audio
{
    internal class AudioStore : ObservableObject, IDisposable, IMMNotificationClient
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private uint _volume;
        public uint Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private bool _isMuted = false;
        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice? device;

        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public AudioStore()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);

            device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.OnVolumeNotification += Device_OnVolumeNotification;

            UpdateValuesByDevice();
        }

        private void UpdateValuesByDevice()
        {
            if (device == null)
            {
                return;
            }

            dispatcher.TryEnqueue(() =>
            {
                Name = device.FriendlyName;
                Volume = (uint)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                IsMuted = device.AudioEndpointVolume.Mute;
                Debug.WriteLine($"UpdateValuesByDevice {Name} {Volume}");
            });
        }

        private void Device_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            UpdateValuesByDevice();
        }

        // IMMNotificationClient implementation for device change notifications
        public void OnDeviceStateChanged(string pwstrDeviceId, DeviceState dwNewState) { }

        public void OnDeviceAdded(string pwstrDeviceId) { }

        public void OnDeviceRemoved(string pwstrDeviceId) { }

        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string pwstrDefaultDeviceId)
        {
            try
            {
                if (device != null)
                {
                    device.AudioEndpointVolume.OnVolumeNotification -= Device_OnVolumeNotification;
                    //device.Dispose();
                }

                var newDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                Debug.WriteLine($"OnDefaultDeviceChanged {device?.FriendlyName} -> {newDevice?.FriendlyName}");
                device = newDevice;
                if (device != null)
                {
                    device.AudioEndpointVolume.OnVolumeNotification += Device_OnVolumeNotification;
                    UpdateValuesByDevice();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDefaultDeviceChanged Exception: {ex}");
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

        public void Dispose()
        {
            deviceEnumerator?.UnregisterEndpointNotificationCallback(this);
            deviceEnumerator?.Dispose();
        }

        public void SetVolume(uint newVolume)
        {
            if (device?.AudioEndpointVolume == null)
            {
                return;
            }

            try
            {
                // Clamp volume between 0 and 100
                newVolume = Math.Max(0, Math.Min(newVolume, 100));

                // Convert from percentage to scalar (0.0 to 1.0)
                float volumeScalar = newVolume / 100.0f;

                device.AudioEndpointVolume.MasterVolumeLevelScalar = volumeScalar;
                Volume = newVolume;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetVolume Exception: {ex}");
            }
        }
    }
}
