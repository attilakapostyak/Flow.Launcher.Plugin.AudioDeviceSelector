using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Flow.Launcher.Plugin.AudioDeviceSelector.Components
{
    public class Settings : INotifyPropertyChanged
    {
        private bool displayFriendlyName;
        private bool displayDeviceName;
        private bool displayDeviceDescription;
        private bool cacheDeviceNames;

        public Settings()
        {
        }

        public bool DisplayFriendlyName
        {
            get => displayFriendlyName;
            set
            {
                displayFriendlyName = value;

                OnPropertyChanged();
            }
        }

        public bool DisplayDeviceName
        {
            get => displayDeviceName;
            set
            {
                displayDeviceName = value;

                OnPropertyChanged();
            }
        }

        public bool DisplayDeviceDescription
        {
            get => displayDeviceDescription;
            set
            {
                displayDeviceDescription = value;

                OnPropertyChanged();
            }
        }

        public bool CacheDeviceNames
        {
            get => cacheDeviceNames;
            set
            {
                cacheDeviceNames = value;

                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
