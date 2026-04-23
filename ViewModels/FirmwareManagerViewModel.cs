using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenKNX.Toolbox;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wpf.Ui;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class FirmwareManagerViewModel : INotifyPropertyChanged
    {
        public static FirmwareManagerViewModel Instanz { get; } = new FirmwareManagerViewModel();

        public ObservableCollection<ApplicationModel> Applications { get; set; } = new();

        private string _selectedApplication = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                _selectedApplication = value;
                Changed("SelectedApplication");
            }
        }
        
        private void Instanz_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender != null && e.PropertyName == "SelectedDevice")
            {
                SelectedApplication = ((MainViewModel)sender).SelectedDevice?.AppId ?? string.Empty;
            }
        }

        public async Task Init()
        {
            MainViewModel.Instanz.PropertyChanged += Instanz_PropertyChanged;
            await UpdateFirmwareList();
            UpdateLocalList();
        }

        public void UpdateLocalList()
        {
            foreach(var app in Applications)
            {
                foreach(var release in app.Releases)
                {
                    string releasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    releasePath = Path.Combine(releasePath, "OpenKNX", "Firmware", app.AppId, release.Version.ToString());

                    release.IsLocalAvailable = Directory.Exists(releasePath);

                    if(release.IsLocalAvailable)
                    {
                        release.ContentModel = ReleaseContentHelper.GetReleaseContent(releasePath, release.Version);
                    }
                }
            }
        }

        public async Task UpdateFirmwareList()
        {
            List<Lib.Models.Application> apps = await GitHubAccess.GetOpenKnxApplicationsAsync();

            Applications.Clear();

            foreach (var app in apps)
            {
                ApplicationModel appModel = new(app);

                foreach(var rel in app.Releases)
                {
                    ReleaseModel releaseModel = new(rel, appModel.AppId);
                    appModel.Releases.Add(releaseModel);
                }

                Applications.Add(appModel);
            }

            UpdateLocalList();
        }

        public void DeleteLocalRelease(ReleaseModel release)
        {
            string releasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            releasePath = Path.Combine(releasePath, "OpenKNX", "Firmware", release.AppId, release.Version.ToString());
            if(Directory.Exists(releasePath))
            {
                try
                {
                    Directory.Delete(releasePath, true);
                    release.IsLocalAvailable = false;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"{Properties.Resources.DeleteFirmwareDirError}\n{ex.Message}", Properties.Resources.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public async Task FlashRelease(Product product)
        {
            CancellationTokenSource token = new CancellationTokenSource();

            FlashSelectDialog flashSelectDialog = new(product.Architecture, product.AppId);

            await MainViewModel.Instanz.ContentDialogService.ShowAsync(
                flashSelectDialog,
                token.Token
            );

            PlatformDevice? device = flashSelectDialog.GetSelectedDevice();
            if (device != null)
            {
                ActionsViewModel.Instanz.AddAction(new FlashAction(device, product));
            }
        }

        [RelayCommand]
        public async Task ReloadFirmwares()
        {
            await UpdateFirmwareList();
        }

        [RelayCommand]
        public void OpenHelp(ApplicationModel application)
        {
            if (string.IsNullOrEmpty(application.HelpThread))
                return;
            Process.Start(new ProcessStartInfo
            {
                FileName = application.HelpThread,
                UseShellExecute = true // wichtig für .NET Core / .NET 5+
            });
        }

        [RelayCommand]
        public void OpenRepo(ApplicationModel application)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://github.com/OpenKNX/{application.Name}",
                UseShellExecute = true // wichtig für .NET Core / .NET 5+
            });
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
