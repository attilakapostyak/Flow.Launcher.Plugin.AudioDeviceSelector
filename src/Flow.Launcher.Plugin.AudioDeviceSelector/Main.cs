using Flow.Launcher.Plugin.AudioDeviceSelector.Audio;
using Flow.Launcher.Plugin.AudioDeviceSelector.Audio.Interop;
using Flow.Launcher.Plugin.AudioDeviceSelector.Components;
using Flow.Launcher.Plugin.AudioDeviceSelector.Views;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Flow.Launcher.Plugin.AudioDeviceSelector
{
    public class Main : IPlugin, IPluginI18n, ISettingProvider
    {
        internal PluginInitContext Context;

        private DateTime lastDeviceUpdateTimeStamp = DateTime.Now;
        private int updateIntervalSeconds = 5;
        private MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();

        private const string imagePath = "Images/speaker.png";

        private SettingsUserControl SettingWindow;
        private Settings settings;
        private bool disposedValue;

        private AudioDevicesManager audioDevicesManager;

        public Control CreateSettingPanel()
        {
            SettingWindow = new SettingsUserControl(Context, settings, audioDevicesManager);
            return SettingWindow;
        }

        public TitleTypeSettings GetTitleTypeSettings(Settings settings)
        {
            if (settings.DisplayFriendlyName)
                return TitleTypeSettings.FriendlyName;
            if (settings.DisplayDeviceName)
                return TitleTypeSettings.DeviceName;
            if (settings.DisplayDeviceDescription)
                return TitleTypeSettings.DeviceDescription;

            return TitleTypeSettings.FriendlyName;
        }

        public List<Result> Query(Query query)
        {
            try
            {
                audioDevicesManager.UpdateDevices();
                var results = new List<Result>();

                var titleType = GetTitleTypeSettings(settings);

                foreach (var device in audioDevicesManager.Devices)
                {
                    string title = string.Empty;
                    string subTitle = string.Empty;
                    switch (titleType)
                    {
                        case TitleTypeSettings.FriendlyName:
                            title = audioDevicesManager.GetDeviceTitle(device.FriendlyName, TitleTypeSettings.FriendlyName);
                            break;
                        case TitleTypeSettings.DeviceName:
                            title = audioDevicesManager.GetDeviceTitle(device.FriendlyName, TitleTypeSettings.DeviceName);
                            subTitle = audioDevicesManager.GetDeviceTitle(device.FriendlyName, TitleTypeSettings.DeviceDescription);
                            break;
                        case TitleTypeSettings.DeviceDescription:
                            title = audioDevicesManager.GetDeviceTitle(device.FriendlyName, TitleTypeSettings.DeviceDescription);
                            subTitle = audioDevicesManager.GetDeviceTitle(device.FriendlyName, TitleTypeSettings.DeviceName);
                            break;
                    }

                    if (string.IsNullOrEmpty(subTitle) || (subTitle != null && subTitle.Trim().Length == 0)) 
                    {
                        subTitle = GetTranslatedPluginTitle();
                    }

                    var result = new Result
                    {
                        Title = title,
                        SubTitle = subTitle,
                        Action = c =>
                        {
                            try
                            {
                                if (!audioDevicesManager.SetDevice(device.FriendlyName))
                                {
                                    // Show Notification Message if device is not found
                                    // Can happen in situations where since FlowLauncher was shown, the device went offline
                                    Context.API.ShowMsg(GetTranslatedPluginTitle(),
                                                            GetTranslatedDeviceNotFoundError(device.FriendlyName));
                                }
                            }
                            catch (Exception)
                            {
                                Context.API.ShowMsg(GetTranslatedPluginTitle(),
                                                        GetTranslatedChangingDeviceError());
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
            settings = Context.API.LoadSettingJsonStorage<Settings>();
            if (!settings.DisplayFriendlyName && !settings.DisplayDeviceName && !settings.DisplayDeviceDescription)
                settings.DisplayFriendlyName = true;

            audioDevicesManager = new AudioDevicesManager();
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
        
        public string GetTranslatedChangingDeviceError()
        {
            return Context.API.GetTranslation("plugin_audiodeviceselector_error_while_changing_device"); ;
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
