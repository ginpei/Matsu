using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matsu.Lib.Audio
{
    internal class AudioStore : ObservableObject
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private float _volume;
        public float Volume
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

        private MMDevice? device = null;
        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public AudioStore() {
            device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            Name = device.DeviceFriendlyName;
            Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            IsMuted = device.AudioEndpointVolume.Mute;
            Debug.WriteLine($"Name: {Name} Volume: {Volume * 100}%  Mute: {IsMuted}");

            device.AudioEndpointVolume.OnVolumeNotification += (data) =>
            {
                dispatcher.TryEnqueue(() =>
                {
                    Volume = data.MasterVolume;
                    IsMuted = data.Muted;
                });
            };
        }

        public void Dispose()
        {
            device?.Dispose();
            device = null;
        }

    }
}
