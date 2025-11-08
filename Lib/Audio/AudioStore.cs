using AudioSwitcher.AudioApi.CoreAudio;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;

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

        private double _volume;
        public double Volume
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

        private CoreAudioController controller = new CoreAudioController();
        private CoreAudioDevice device;
        private IDisposable? volumeSubscription;

        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public AudioStore() {
            device = controller.DefaultPlaybackDevice;
            ResetDevice(device);

            controller.AudioDeviceChanged.Subscribe((e) =>
            {
                var newDevice = controller.DefaultPlaybackDevice;
                if (newDevice != device)
                {
                    ResetDevice(newDevice);
                    device = newDevice;
                }
            });
        }

        private void ResetDevice(CoreAudioDevice device)
        {
            Debug.WriteLine($"DefaultChanged: {device.FullName}");

            volumeSubscription?.Dispose();
            volumeSubscription = device.VolumeChanged.Subscribe((e) =>
            {
                dispatcher.TryEnqueue(() =>
                {
                    Name = device.FullName;
                    Volume = device.Volume;
                    IsMuted = device.IsMuted;
                });
            });

            dispatcher.TryEnqueue(() =>
            {
                Debug.WriteLine($"audio store: {device.FullName}");
                Name = device.FullName;
                Volume = device.Volume;
                IsMuted = device.IsMuted;
            });
        }

        public void Dispose()
        {
            volumeSubscription?.Dispose();
            //device?.Dispose();
            controller.Dispose();
        }

    }
}
