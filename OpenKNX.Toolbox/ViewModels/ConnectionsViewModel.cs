using CommunityToolkit.Mvvm.Input;
using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using Kaenx.Konnect.Telegram.IP.DIB;
using Octokit;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class ConnectionsViewModel : INotifyPropertyChanged
    {
        public static ConnectionsViewModel Instanz { get; } = new ConnectionsViewModel();
        public ObservableCollection<ConnectionModel> Connections { get; private set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private IpKnxConnection _conn;
        private Timer _timer;

        public ConnectionsViewModel()
        {
            // _conn = KnxFactory.CreateRouting(UnicastAddress.FromString("0.0.1"));
            _conn = KnxFactory.CreateTunnelingUdp("224.0.23.12", 3671);
            _conn.OnReceivedService += _conn_OnReceivedService;

            _timer = new Timer(SearchCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        private void _conn_OnReceivedService(IpTelegram ipTelegram)
        {
            if (ipTelegram is SearchResponse sr)
            {
                Debug.WriteLine($"SearchResponse: {sr.GetDeviceInfo()?.FriendlyName ?? "unbenannt"}");

                SupportedServiceFamilies? svcFamilies = sr.GetSupportedServiceFamilies();
                if (svcFamilies == null)
                    return;

                if (svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Tunneling) > 0)
                {
                    int tunnelingVersion = svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Tunneling);
                    FoundInterface(sr, true, false);

                    if (tunnelingVersion >= 2)
                    {
                        FoundInterface(sr, true, true);
                    }
                }

                if (svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Routing) > 0)
                {
                    int routingVersion = svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Routing);
                    FoundInterface(sr, false, true);
                }
            }
        }

        private void FoundInterface(SearchResponse searchResponse, bool isTunnel, bool isTCP)
        {
            DeviceInfo? info = searchResponse.GetDeviceInfo();
            HpaiContent? hpai = searchResponse.GetEndpoint();

            if (info == null || hpai == null)
                return;

            string serial = BitConverter.ToString(info.SerialNumber).Replace("-", "");
            Console.WriteLine($" -> {hpai.Endpoint,-22} ({info.UnicastAddress,-9}) [{info.FriendlyName}]");

            ConnectionModel? conn = Connections.SingleOrDefault(c => c.SerialNumber == serial && c.IsTCP == isTCP && c.IsTunnel == isTunnel);
            if (conn == null)
            {
                conn = new ConnectionModel();
                conn.SerialNumber = serial;
                conn.FriendlyName = info.FriendlyName;
                conn.Counter = 2;
                conn.IsTunnel = isTunnel;
                conn.IsTCP = isTCP;
                conn.EndPoint = hpai.Endpoint;
                conn.UnicastAddress = info.UnicastAddress;
                Connections.Add(conn);
            }
            else
            {
                conn.Counter = 2;
            }
        }

        private void SearchCallback(object? state)
        {
            foreach(ConnectionModel conn in Connections.ToList())
            {
                if(conn.IsManuallyAdded)
                    continue;
                conn.Counter--;
                if (conn.Counter == 0)
                    Connections.Remove(conn);
            }

            SearchRequest request = new SearchRequest(_conn.GetLocalEndpoint());
            _ = _conn.SendAsync(request);
        }

        [RelayCommand]
        public async Task AddConnection()
        {
            CancellationTokenSource token = new CancellationTokenSource();
            NewConnectionDialog newConnectionDialog = new NewConnectionDialog();

            await MainViewModel.Instanz.ContentDialogService.ShowAsync(
                newConnectionDialog,
                token.Token
            );

            if (newConnectionDialog.GetAddedItem() is ConnectionModel conn)
            {
                Connections.Add(conn);
            }
        }


        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
