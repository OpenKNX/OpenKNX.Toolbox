using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace OpenKNX.Toolbox.Models
{
    public partial class ReleaseModel: INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string NoteUrl { get; set; } = string.Empty;
        public bool IsPrerelease { get; set; } = false;
        public SemanticVersion Version { get; set; }
        public string VersionString { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;

        private bool _isLocalAvailable = false;
        public bool IsLocalAvailable
        {
            get { return _isLocalAvailable; }
            set
            {
                _isLocalAvailable = value;
                Changed("IsLocalAvailable");
            }
        }

        private ReleaseContentModel? _contentModel = null;
        public ReleaseContentModel? ContentModel
        {
            get { return _contentModel; }
            set
            {
                _contentModel = value;
                Changed("ContentModel");
            }
        }

        public ReleaseModel(AppRelease release, string appId)
        {
            Name = release.Name;
            PublishedAt = release.PublishedAt;
            FileUrl = release.FileUrl;
            NoteUrl = release.NotesUrl;
            IsPrerelease = release.IsPrerelease;
            Version = release.Version ?? new SemanticVersion(0);
            VersionString = Version?.ToString() ?? Properties.Resources.UnknownVersion;
            AppId = appId;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        [RelayCommand]
        public void DownloadRelease()
        {
            string destination = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            destination = Path.Combine(destination, "OpenKNX", "Firmware", AppId, Version.ToString());
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            destination = Path.Combine(destination, Name);
            ActionsViewModel.Instanz.AddAction(new DownloadAction($"Download {Name}", FileUrl, destination));
        }

        [RelayCommand]
        public void DeleteRelease()
        {
            FirmwareManagerViewModel.Instanz.DeleteLocalRelease(this);
        }

        [RelayCommand]
        public void OpenReleaseNotes()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = NoteUrl,
                UseShellExecute = true // wichtig für .NET Core / .NET 5+
            });
        }

        [RelayCommand]
        public void CreateKnxprod()
        {
            if (ContentModel == null)
            {
                System.Windows.MessageBox.Show(Properties.Resources.ReleaseNotLocal, Properties.Resources.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select destination for knxprod file";
            saveFileDialog.Filter = "KNX Product File (*.knxprod)|*.knxprod";
            saveFileDialog.FileName = Path.GetFileName($"{ContentModel.ReleaseName}_{Version.ToString()}");

            if (saveFileDialog.ShowDialog() == true)
            {
                ActionsViewModel.Instanz.AddAction(new KnxprodAction($"{ContentModel.ReleaseName} {Version.ToString()}", ContentModel.XmlFile, saveFileDialog.FileName));
            }

        }
    }
}
