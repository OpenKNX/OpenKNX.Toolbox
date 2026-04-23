using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes.Actions
{
    public class DownloadAction : IAction, INotifyPropertyChanged
    {
        public string ActionName { get; } = Properties.Resources.DownloadRelease;

        public string Name { get; }

        private int _progress = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                Changed("Progress");
            }
        }

        private bool _isIndeterminate = false;
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                Changed("IsIndeterminate");
            }
        }

        private string _downloadUrl = string.Empty;
        private string _destinationPath = string.Empty;
        private CancellationToken _token = default;


        public DownloadAction(string name, string url, string destination)
        {
            Name = name;
            _downloadUrl = url;
            _destinationPath = destination;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task Begin(CancellationToken token)
        {
            _token = token;

            if (_destinationPath == null)
                throw new DirectoryNotFoundException($"Could not finde destination Path: {_destinationPath}");

            IsIndeterminate = true;

            if (File.Exists(_destinationPath))
            {
                File.Delete(_destinationPath);
            }


            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(_downloadUrl, _token);
                if (_token.IsCancellationRequested)
                    return;
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(_destinationPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs, _token);
                    if (_token.IsCancellationRequested)
                        return;
                }
            }

            string? targetPath = Path.GetDirectoryName(_destinationPath);
            if (targetPath == null)
                throw new DirectoryNotFoundException($"Could not find Directory: {targetPath}");
            System.IO.Compression.ZipFile.ExtractToDirectory(_destinationPath, targetPath);
            File.Delete(_destinationPath);

            FirmwareManagerViewModel.Instanz.UpdateLocalList();
        }
    }
}
