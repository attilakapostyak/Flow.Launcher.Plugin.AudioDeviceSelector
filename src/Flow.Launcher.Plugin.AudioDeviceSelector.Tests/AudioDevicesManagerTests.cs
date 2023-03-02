using Flow.Launcher.Plugin.AudioDeviceSelector.Audio;
using System;
using Xunit;

namespace Flow.Launcher.Plugin.AudioDeviceSelector.Tests
{
    public class AudioDevicesManagerTests
    {
        [Theory]
        [InlineData("", TitleTypeSettings.FriendlyName, "")]
        [InlineData("Speakers (SRS-XB13 Stereo)", TitleTypeSettings.FriendlyName, "Speakers (SRS-XB13 Stereo)")]
        [InlineData("Speakers", TitleTypeSettings.DeviceName, "Speakers")]
        [InlineData("SRS-XB13 Stereo", TitleTypeSettings.DeviceDescription, "SRS-XB13 Stereo")]
        [InlineData("Speakers (SRS-XB13 Stereo)", TitleTypeSettings.DeviceName, "Speakers")]
        [InlineData("Speakers (SRS-XB13 Stereo)", TitleTypeSettings.DeviceDescription, "SRS-XB13 Stereo")]
        [InlineData("Speakers(SRS-XB13 Stereo)", TitleTypeSettings.DeviceName, "Speakers")]
        [InlineData("Speakers(SRS-XB13 Stereo)", TitleTypeSettings.DeviceDescription, "SRS-XB13 Stereo")]
        public void Test_GetDeviceTitle(string friendlyName, TitleTypeSettings titleType, string expectedResult)
        {
            var obj = new AudioDevicesManager();

            var result = obj.GetDeviceTitle(friendlyName, titleType);

            Assert.Equal(result, expectedResult);
        }
    }
}