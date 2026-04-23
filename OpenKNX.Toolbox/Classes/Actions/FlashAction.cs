using Microsoft.Win32;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes.Actions
{
    public class FlashAction : IAction, INotifyPropertyChanged
    {
        public string ActionName { get; } = Properties.Resources.FlashRelease;

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

        private PlatformDevice _device;
        private Product _product;
        private CancellationToken _token = default;

        public FlashAction(PlatformDevice device, Product product)
        {
            Name = $"{device.Name} [{product.AppId} - {product.Version}]";
           _product = product;
            _device = device;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task Begin(CancellationToken token)
        {
            _token = token;

            IsIndeterminate = true;
            await Task.Delay(1000);

            await PlatformHelper.DoUpload(_device, _product.FirmwareFile, new Progress<KeyValuePair<long, long>>((v) =>
            {
                if (v.Value > 0)
                {
                    IsIndeterminate = false;
                    Progress = (int)((v.Key * 100) / v.Value);
                }
                else
                {
                    IsIndeterminate = true;
                }
            }), _token);
        }
    }
}
