using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace OpenKNX.Toolbox.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel? _instanz;
        public static MainViewModel Instanz
        {
            get
            {
                if (_instanz == null)
                    _instanz = new MainViewModel();
                return _instanz;
            }
        }

        public ActionsViewModel ActionsViewModel
        {
            get { return ActionsViewModel.Instanz; }
        }

        public ConnectionsViewModel ConnectionsViewModel
        {
            get { return ConnectionsViewModel.Instanz; }
        }

        public DeviceManagerViewModel DeviceManagerViewModel
        {
            get { return DeviceManagerViewModel.Instanz; }
        }

        public ContentDialogService ContentDialogService { get; } = new ContentDialogService();
        public SnackbarService SnackbarService { get; } = new SnackbarService();

        public string Title
        {
            get {
                string title = "OpenKNX Toolbox";
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly == null) return title;
                var vers = assembly.GetName().Version;
                if (vers == null) return title;
                title += " - v" + string.Join('.', vers.ToString().Split('.').Take(4));
                return title;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task Init()
        {
            ActionsViewModel.Init();
            await FirmwareManagerViewModel.Instanz.Init();
            DeviceManagerViewModel.Instanz.Init();
        }

        private DeviceModel? _selectedDevice;
        public DeviceModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                Changed("SelectedDevice");
            }
        }

        public void SetContentPresenterDialog(ContentPresenter presenter)
        {
            ContentDialogService.SetDialogHost(presenter);
        }

        public void SetContentPresenterSnackbar(SnackbarPresenter presenter)
        {
            SnackbarService.SetSnackbarPresenter(presenter);
        }

        public void ShowError(string title, string message, int timeout = 5)
        {
            TimeSpan _timeout = TimeSpan.FromSeconds(timeout);
            SnackbarService.Show(title, message, ControlAppearance.Caution, null, _timeout);
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
