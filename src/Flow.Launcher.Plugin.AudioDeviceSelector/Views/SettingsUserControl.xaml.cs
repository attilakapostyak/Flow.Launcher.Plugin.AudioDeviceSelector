using Flow.Launcher.Plugin.AudioDeviceSelector.Audio;
using Flow.Launcher.Plugin.AudioDeviceSelector.Components;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flow.Launcher.Plugin.AudioDeviceSelector.Views
{
    /// <summary>
    /// Interaction logic for SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : UserControl
    {
        public Settings Settings { get; set; }

        private PluginInitContext Context;
        private AudioDevicesManager audioDevicesManager;

        public SettingsUserControl(PluginInitContext _context, Settings _settings, AudioDevicesManager _audioDevicesManager)
        {
            Context = _context;
            Settings = _settings;
            audioDevicesManager = _audioDevicesManager;

            InitializeComponent();

            var pluginName = Context.API.GetTranslation("plugin_audiodeviceselector_plugin_name");


            var devices = audioDevicesManager.GetDevices();
            var deviceFriendlyName = "Headphones (WH-XB910N Stereo)";
            var deviceDeviceName = string.Empty;
            var deviceDeviceDescription = string.Empty;
            if (devices.Count > 0)
            {
                deviceFriendlyName = devices[0].FriendlyName;
            }
            deviceDeviceName = audioDevicesManager.GetDeviceTitle(deviceFriendlyName, TitleTypeSettings.DeviceName);
            deviceDeviceDescription = audioDevicesManager.GetDeviceTitle(deviceFriendlyName, TitleTypeSettings.DeviceDescription);

            displayFriendlyNameTitleTextBlock.Text = $"{deviceFriendlyName}";
            displayFriendlyNameSubtitleTextBlock.Text = $"{pluginName}";

            displayDeviceNameTitleTextBlock.Text = $"{deviceDeviceName}";
            displayDeviceNameSubtitleTextBlock.Text = $"{deviceDeviceDescription}";

            displayDeviceDescriptionTitleTextBlock.Text = $"{deviceDeviceDescription}";
            displayDeviceDescriptionSubtitleTextBlock.Text = $"{deviceDeviceName}";

            DeviceNameDisplayModeLabel.Content = Context.API.GetTranslation("plugin_audiodeviceselector_settings_devicename_displaymode");
            FriendlyNameOption.Content = Context.API.GetTranslation("plugin_audiodeviceselector_settings_friendlynameoption");
            DeviceNameOption.Content = Context.API.GetTranslation("plugin_audiodeviceselector_settings_devicenameoption");
            DeviceDescriptionOption.Content = Context.API.GetTranslation("plugin_audiodeviceselector_settings_devicedescriptionoption");

            CacheDeviceNamesCheckbox.Content =
                Context.API.GetTranslation("plugin_audiodeviceselector_settings_cache_device_names_checkbox");
            CacheDeviceNamesExplanation.Text =
                Context.API.GetTranslation("plugin_audiodeviceselector_settings_cache_device_names_explanation");
        }
    }
}
