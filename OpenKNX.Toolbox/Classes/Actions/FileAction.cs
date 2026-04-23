using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Connections;
using KnxFileTransferClient.Lib;
using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes.Actions
{
    public class FileAction : IAction, INotifyPropertyChanged
    {
        public string ActionName { get; }

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

        private FileActionTypes _actionType;
        private string _source = string.Empty;
        private string _destination = string.Empty;
        private ConnectionModel _connection;
        private UnicastAddress _remoteAddress;
        private bool _isFile = true;
        private int _maxLength = 50;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FileAction(UnicastAddress remoteAddress, ConnectionModel conn, string source, string destination, bool isFile, FileActionTypes actionType)
        {
            _connection = conn;
            _source = source;
            _destination = destination;
            _actionType = actionType;
            _remoteAddress = remoteAddress;
            _isFile = isFile;

            switch (actionType)
            {
                case FileActionTypes.Delete:
                    ActionName = Properties.Resources.DeleteFile;
                    Name = string.Format(Properties.Resources.DeleteFileDisplay, _source);
                    break;

                case FileActionTypes.Download:
                    ActionName = Properties.Resources.DownloadFile;
                    Name = _source;
                    break;

                case FileActionTypes.Upload:
                    ActionName = Properties.Resources.UploadFile;
                    Name = _destination;
                    break;

                default:
                    ActionName = Properties.Resources.UnknownAction;
                    Name = Properties.Resources.UnknownAction;
                    break;
            }
        }

        public async Task Begin(CancellationToken token)
        {
            await Task.Delay(3000);
            IKnxConnection conn = GetConnection();
            await conn.Connect();
            BusDevice dev = new BusDevice(_remoteAddress, conn);
            await dev.ConnectIndividual();
            FileTransferClient client = new FileTransferClient(dev);

            switch (_actionType)
            {
                case FileActionTypes.Delete:
                    await Delete(client);
                    break;

                case FileActionTypes.Download:
                    await Download(client);
                    break;

                case FileActionTypes.Upload:
                    await Upload(client);
                    break;

                default:
                    MainViewModel.Instanz.ShowError("File Action Error", Properties.Resources.UnknownActionType + _actionType.ToString());
                    break;
            }

            await conn.Disconnect();
        }

        private IKnxConnection GetConnection()
        {
            if(!_connection.IsTunnel)
                return KnxFactory.CreateRouting(UnicastAddress.FromString("0.0.1"));
            else
            {
                if (_connection.IsTCP)
                    return KnxFactory.CreateTunnelingTcp(_connection.EndPoint);
                else
                    return KnxFactory.CreateTunnelingUdp(_connection.EndPoint);
            }
        }

        private async Task Delete(FileTransferClient client)
        {
            if (_isFile)
                await client.FileDelete(_source, false);
            else
                await client.DirDelete(_source, false);
        }

        private async Task Download(FileTransferClient client)
        {
            await client.FileDownload(_source, _destination, _maxLength, false);
        }

        private async Task Upload(FileTransferClient client)
        {
            await client.FileUpload(_source, _destination, _maxLength, 1, false);
        }
    }
}
