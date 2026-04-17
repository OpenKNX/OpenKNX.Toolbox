using CommunityToolkit.Mvvm.Input;
using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using Kaenx.Konnect.Telegram.IP.DIB;
using KnxFileTransferClient.Lib;
using Octokit;
using OpenKNX.Toolbox.Classes;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Management.Automation.Language;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class FileManagerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string RemoteAddress { get; set; } = string.Empty;
        public UnicastAddress RemoteAddressUni = UnicastAddress.FromString("0.0.0");

        private bool _isDragging = false;
        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                _isDragging = value;
                Changed(nameof(IsDragging));
            }
        }

        public ObservableCollection<FileModel> Items { get; set; } = new ObservableCollection<FileModel>();

        private ConnectionModel? _connection;

        public ConnectionModel? GetConnectionModel()
        {
            return _connection;
        }


        [RelayCommand]
        public void Delete(FileModel file)
        {
            if (_connection == null)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", Properties.Resources.NoGatewayConnection);
                return;
            }
            ActionsViewModel.Instanz.AddAction(new FileAction(RemoteAddressUni, _connection, file.FullPath, string.Empty, file.IsFile, FileActionTypes.Delete));
        }

        [RelayCommand]
        public void Download(FileModel file)
        {
            if(!file.IsFile)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", Properties.Resources.SelectedItemNotFile);
                return;
            }
            if (_connection == null)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", Properties.Resources.NoGatewayConnection);
                return;
            }
            ActionsViewModel.Instanz.AddAction(new FileAction(RemoteAddressUni, _connection, file.FullPath, @"C:\Users\Mike\Desktop\sources\test.txt", true, FileActionTypes.Download));
        }

        [RelayCommand]
        public async Task ReloadFiles()
        {
            try
            {
                RemoteAddressUni = UnicastAddress.FromString(RemoteAddress);
            } catch
            {
                MainViewModel.Instanz.ShowError("FileManager Error", Properties.Resources.InvalidAddress);
                return;
            }

            ObjectSelectDialog selectDialog = new ObjectSelectDialog(Properties.Resources.GatewaySelection, ConnectionsViewModel.Instanz.Connections.Cast<object>().ToList());

            CancellationTokenSource token = new CancellationTokenSource();
            await MainViewModel.Instanz.ContentDialogService.ShowAsync(
                selectDialog,
                token.Token
            );

            ConnectionModel? connection = selectDialog.GetSelectedItem() as ConnectionModel;
            if (connection == null)
            {
                return;
            }

            _connection = connection;

            Items.Clear();

            IpKnxConnection _conn = KnxFactory.CreateTunnelingUdp(connection.EndPoint);
            await _conn.Connect();

            BusDevice busDevice = new BusDevice(RemoteAddressUni.ToString(), _conn);
            await busDevice.ConnectIndividual();

            FileTransferClient client = new FileTransferClient(busDevice);

            await GetItemsForFolder(client, "/", Items);

            await busDevice.Disconnect();
            await _conn.Disconnect();
        }

        public async Task GetItemsForFolder(FileTransferClient client, string path, ObservableCollection<FileModel> subItems)
        {
            List<FileTransferPath> items = await client.List(path, false);

            foreach(var item in items)
            {
                FileModel model = new FileModel()
                {
                    Name = item.Name,
                    IsFile = item.IsFile,
                    FullPath = path.EndsWith("/") ? path + item.Name : path + "/" + item.Name
                };
                subItems.Add(model);

                if (!model.IsFile)
                    await GetItemsForFolder(client, model.FullPath, model.Items);

                // Fix für CS0411: Typargumente explizit angeben
                subItems.Sort();
            }
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
