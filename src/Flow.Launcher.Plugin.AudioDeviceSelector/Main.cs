using Flow.Launcher.Plugin.AudioDeviceSelector.Audio.Interop;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.AudioDeviceSelector
{
    public class Main : IPlugin, IPluginI18n
    {
        internal PluginInitContext Context;

        private List<MMDevice> devices = null;
        private DateTime lastDeviceUpdateTimeStamp = DateTime.Now;
        private int updateIntervalSeconds = 5;
        private MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();

        private const string imagePath = "Images/speaker.png";

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

            return false;
        }

        public List<Result> Query(Query query)
        {
            try
            {
                UpdateDevices();
                var results = new List<Result>();

                foreach (var device in devices)
                {
                    var result = new Result
                    {
                        Title = device.FriendlyName,
                        SubTitle = GetTranslatedPluginTitle(),
                        Action = c =>
                        {
                            if (!SetDevice(device.FriendlyName))
                            {
                                // Show Notification Message if device is not found
                                // Can happen in situations where since FlowLauncher was shown, the device went offline
                                Context.API.ShowMsg(GetTranslatedPluginTitle(),
                                                        GetTranslatedDeviceNotFoundError(device.FriendlyName));
                            }
                            return true;
                        },
                        IcoPath = imagePath
                    };

                    results.Add(result);
                }

                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return SingleResult(
                    "There was an error while processing the request",
                    e.GetBaseException().Message
                );
            }
        }

        // Returns a list with a single result
        private static List<Result> SingleResult(string title, string subtitle = "", Action action = default, bool hideAfterAction = true) =>
            new()
                {
                    new Result()
                    {
                        Title = title,
                        SubTitle = subtitle,
                        IcoPath = imagePath ,
                        Action = _ =>
                        {
                            action?.Invoke();
                            return hideAfterAction;
                        }
                    }
                };

        public void Init(PluginInitContext context)
        {
            Context = context;
        }

        public string GetTranslatedDeviceNotFoundError(string deviceName)
        {
            var message = Context.API.GetTranslation("plugin_audiodeviceselector_device_not_found");
            if (string.IsNullOrEmpty(message))
            {
                return "Device not found";
            }

            return string.Format(message, deviceName);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("plugin_audiodeviceselector_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("plugin_audiodeviceselector_plugin_description");
        }
    }
}
