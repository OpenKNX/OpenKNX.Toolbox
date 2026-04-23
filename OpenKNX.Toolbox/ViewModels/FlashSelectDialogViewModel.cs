using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public class FlashSelectDialogViewModel : INotifyPropertyChanged
    {
        private PlatformDevice? _selectedDevice = null;
        public PlatformDevice? SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                Changed("SelectedDevice");
                CanContinue = _selectedDevice != null;
            }
        }

        private bool _isSearching = false;
        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                _isSearching = value;
                Changed("IsSearching");
            }
        }

        private bool _canContinue = false;
        public bool CanContinue
        {
            get { return _canContinue; }
            set
            {
                _canContinue = value;
                Changed("CanContinue");
            }
        }

        public ObservableCollection<PlatformDevice> Devices { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task UpdateDeviceList(ArchitectureType arch, string appId)
        {
            IsSearching = true;
            await PlatformHelper.GetDevices(Devices, arch);

            foreach (DeviceModel device in DeviceManagerViewModel.Instanz.DeviceModels)
            {
                if (device.AppId == appId)
                {
                    if (device.IsIP)
                        Devices.Add(new(ArchitectureType.ESP32, device.Name, $"{device.IP}:{device.Port}", "ota"));
                    else
                        Devices.Add(new(ArchitectureType.ESP32, device.Name, device.Address, "bus"));
                }
            }

            IsSearching = false;
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
