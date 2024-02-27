using Flow.Launcher.Plugin.AudioDeviceSelector.Audio.Interop;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.AudioDeviceSelector.Components;
using NAudio.CoreAudioApi.Interfaces;
using PropertyKeys = NAudio.CoreAudioApi.PropertyKeys;

namespace Flow.Launcher.Plugin.AudioDeviceSelector.Audio
{
    public class AudioDevicesManager : IMMNotificationClient, IDisposable
    {
        public Settings Settings { get; }
        
        private List<MMDevice> devices = null;
        private DateTime lastDeviceUpdateTimeStamp = DateTime.Now;
        private int updateIntervalSeconds = 5;
        private MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        private readonly Dictionary<string, string> idToNameMap = new();

        public List<MMDevice> Devices 
        { 
            get {
                if (devices == null)
                    devices = GetDevices();

                return devices; 
            } 
        }

        public AudioDevicesManager(Settings settings) {
            Settings = settings;
            if (Settings.CacheDeviceNames) {
                deviceEnumerator.RegisterEndpointNotificationCallback(this);
            }

            Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(Settings.CacheDeviceNames)) return;
            if (Settings.CacheDeviceNames)
                deviceEnumerator.RegisterEndpointNotificationCallback(this);
            else {
                deviceEnumerator.UnregisterEndpointNotificationCallback(this);
                idToNameMap.Clear();
            }
        }

        public string GetDeviceNameFromCache(MMDevice device) {
            var id = device.ID;
            if (idToNameMap.ContainsKey(id)) {
                return idToNameMap[id];
            }

            var name = device.FriendlyName;
            idToNameMap[id] = name;
            return name;
        }
        
        public List<MMDevice> GetDevices()
        {
            var datetime1 = DateTime.Now;
            var devices = new List<MMDevice>();

            var endpoints = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var endpoint in endpoints)
            {
                if (Settings.CacheDeviceNames) {
                    // Puts the name in cache if it's not there yet
                    GetDeviceNameFromCache(endpoint);
                }

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

        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }

        public void OnDeviceAdded(string pwstrDeviceId) { }

        public void OnDeviceRemoved(string deviceId) {
            idToNameMap.Remove(deviceId);
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) {
            if (!Settings.CacheDeviceNames) return;
            if (key.propertyId != PropertyKeys.PKEY_Device_FriendlyName.propertyId &&
                key.propertyId != PropertyKeys.PKEY_DeviceInterface_FriendlyName.propertyId)
                return;
            var device = deviceEnumerator.GetDevice(pwstrDeviceId);
            idToNameMap[device.ID] = device.FriendlyName;
        }

        public void Dispose() {
            Settings.PropertyChanged -= SettingsOnPropertyChanged;
            if (!Settings.CacheDeviceNames) return;
            deviceEnumerator.UnregisterEndpointNotificationCallback(this);
        }
    }
}
