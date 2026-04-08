using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Localization;

namespace OpenKNX.Toolbox.ViewModels;

public partial class TerminalViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<PlatformDevice> Devices { get; set; } = new ();

    private PlatformDevice? _selectedPlatformDevice { get; set; }
    public PlatformDevice? SelectedPlatformDevice
    {
        get { return _selectedPlatformDevice; }
        set { 
            _selectedPlatformDevice = value;
            NotifyPropertyChanged("SelectedPlatformDevice");
            CanOpenPutty = value != null;
        }
    }

    private bool _canOpenPutty = false;
    public bool CanOpenPutty
    {
        get { return _canOpenPutty; }
        set {
            _canOpenPutty = value;
            NotifyPropertyChanged("CanOpenPutty");
        }
    }

    public async Task UpdatePlatformDevices()
    {
        Devices.Clear();
        foreach (var device in await PlatformHelper.GetDevices())
            Devices.Add(device);
    }
    
    public async void OpenPutty()
    {
        string url = $"putty -serial {SelectedPlatformDevice?.Path} -sercfg 115200,8,n,1,N";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try {
                using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
                proc.Start();
                return;
            } catch (Exception ex) {
                var box = MessageBoxManager.GetMessageBoxStandard(Localizer.Instance.Strings.Error, Localizer.Instance.Strings.ErrorStartingPutty + "\r\n\r\n" + ex.Message, ButtonEnum.Ok, Icon.Error);
                await box.ShowWindowDialogAsync(Views.MainWindow.Instance);
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged; 
    private void NotifyPropertyChanged(string propertyName = "")  
    {  
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }  
}