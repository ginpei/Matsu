using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matsu.Lib.Agent
{
    internal class PrototypeAgent
    {
        private readonly Audio.AudioStore audioStore = new();
        private readonly Wifi.WifiStore wifiStore = new();

        public PrototypeAgent()
        {
            wifiStore.PropertyChanged += WifiStore_PropertyChanged;
        }

        private void WifiStore_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Wifi.WifiStore s)
            {
                if (e.PropertyName == "Ssid")
                {
                    if (s.Ssid == "dlink-8CD6-5GHz")
                    {
                        Debug.WriteLine("OK!");
                        audioStore.SetMute(false);
                    }
                    else
                    {
                        Debug.WriteLine($"Uh ({s.Ssid})");
                        audioStore.SetMute(true);
                    }
                }
            }
        }
    }
}
