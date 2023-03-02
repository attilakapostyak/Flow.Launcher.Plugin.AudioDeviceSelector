using Flow.Launcher.Plugin.AudioDeviceSelector.Audio.Interop;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.AudioDeviceSelector.Audio
{
    public class AudioDevicesManager
    {
        private List<MMDevice> devices = null;
        private DateTime lastDeviceUpdateTimeStamp = DateTime.Now;
        private int updateIntervalSeconds = 5;
        private MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();

        public List<MMDevice> Devices 
        { 
            get {
                if (devices == null)
                    devices = GetDevices();

                return devices; 
            } 
        }
        public List<MMDevice> GetDevices()
        {
            var datetime1 = DateTime.Now;
            var devices = new List<MMDevice>();

            var endpoints = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var endpoint in endpoints)
            {
                devices.Add(endpoint);
            }

            return devices;
        }

        public void UpdateDevices()
        {
            DateTime currentTime = DateTime.Now;
            if (devices == null || (currentTime - lastDeviceUpdateTimeStamp).TotalSeconds > updateIntervalSeconds)
            {
                devices = GetDevices();
                lastDeviceUpdateTimeStamp = currentTime;
            }
        }

        public bool SetDevice(string deviceFriendlyName)
        {
            var devices = GetDevices();
            var device = devices.Find(d => d.FriendlyName == deviceFriendlyName);
            if (device == null)
                return false;

            var policy = new PolicyConfigClientWin7();
            policy.SetDefaultEndpoint(device.ID, ERole.eMultimedia);

            return true;
        }

        public string GetDeviceTitle(string friendyName, TitleTypeSettings titleType)
        {
            Regex regex = new Regex("^(?<deviceName>.*?)\\s?(?:\\((?<description>.*?)\\))?$");
            MatchCollection matches = regex.Matches(friendyName);

            if (matches.Count > 0)
            {
                var match = matches[0];

                if (match.Success)
                {
                    switch (titleType)
                    {
                        case TitleTypeSettings.FriendlyName:
                            return friendyName;
                        case TitleTypeSettings.DeviceName:
                            var title = match.Groups["deviceName"].Value;
                            if (!string.IsNullOrEmpty(title))
                            {
                                return match.Groups["deviceName"].Value;
                            }
                            break;
                        case TitleTypeSettings.DeviceDescription:
                            title = match.Groups["description"].Value;
                            if (string.IsNullOrEmpty(title))
                            {
                                return match.Groups["deviceName"].Value;
                            }

                            return title;
                    }
                }
            }

            return friendyName;
        }
    }
}
