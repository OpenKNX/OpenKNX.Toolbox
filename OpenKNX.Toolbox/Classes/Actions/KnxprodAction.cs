using Microsoft.Win32;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes.Actions
{
    public class KnxprodAction : IAction, INotifyPropertyChanged
    {
        public string ActionName { get; } = Properties.Resources.CreateKnxprod;

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

        private string _xmlPath = string.Empty;
        private string _outputPath = string.Empty;
        private CancellationToken _token = default;


        public KnxprodAction(string name, string xmlPath, string output)
        {
            Name = string.Format(Properties.Resources.CreateKnxprodDisplay, name);
            _xmlPath = xmlPath;
            _outputPath = output;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task Begin(CancellationToken token)
        {
            _token = token;

            if (_outputPath == null)
                throw new DirectoryNotFoundException($"Could not finde destination Path: {_outputPath}");

            IsIndeterminate = true;

            if (File.Exists(_outputPath))
                File.Delete(_outputPath);

            string? workingDir = Path.GetDirectoryName(Path.GetFullPath(_xmlPath));

            if (workingDir == null)
                throw new Exception("Could not retrieve workingdir: " + _xmlPath);

            await Sign.SignHelper.ExportKnxprodAsync(workingDir, _outputPath, _xmlPath, "", false, false, string.Empty, _token);
        }
    }
}
