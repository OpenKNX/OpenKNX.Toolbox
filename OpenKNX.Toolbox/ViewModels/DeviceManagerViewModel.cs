using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Telegram.IP;
using Newtonsoft.Json.Linq;
using Octokit;
using OpenKNX.Toolbox.Classes;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Management.Automation;
using System.Net;
using Tmds.MDns;
using Wpf.Ui.Controls;

namespace OpenKNX.Toolbox.ViewModels
{
    class DeviceManagerViewModel : INotifyPropertyChanged
    {
        public static DeviceManagerViewModel Instanz { get; } = new DeviceManagerViewModel();

        public ObservableCollection<DeviceModel> DeviceModels { get; } = new ObservableCollection<DeviceModel>();

        private DeviceModel? _selectedDevice;
        public DeviceModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                Changed("SelectedDevice");
                FirmwareManagerViewModel.Instanz.SelectedApplication = value?.AppId ?? string.Empty;
            }
        }

        private int _updatesAvailable = 0;
        public int UpdatesAvailable
        {
            get => _updatesAvailable;
            set
            {
                _updatesAvailable = value;
                Changed("UpdatesAvailable");
            }
        }

        ServiceBrowser? _serviceBrowser;

        private Dictionary<string, string> AppToImage = new Dictionary<string, string>()
        {
            { "$A11E", "https://github.com/OpenKNX/OpenKNX/wiki/media/devices/REG1-Eth-V1.thumb.jpg" },
            { "$A401", "https://github.com/OpenKNX/OpenKNX/wiki/media/devices/REG1-Dali.thumb.jpg" }
        };

        private List<SearchResponse> _searchResponses = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public async void Init()
        {
            _serviceBrowser = new ServiceBrowser();
            _serviceBrowser.ServiceAdded += DeviceFound;
            _serviceBrowser.StartBrowse("_openknx._tcp");

            DeviceModels.CollectionChanged += DeviceModels_CollectionChanged;

            //IpKnxConnection _conn = KnxFactory.CreateTunnelingUdp(new(IPAddress.Parse("224.0.23.12"), 3671));
            //SearchRequest req = new SearchRequest(_conn.GetLocalEndpoint());
            //await _conn.SendAsync(req);
            //_conn.Dispose();

            //await Task.Delay(1000);

            //_conn = KnxFactory.CreateTunnelingUdp("192.168.178.144", 3671);
            //await _conn.Connect();

            //BusDevice busDevice = new BusDevice("1.1.30", _conn);
            //await busDevice.Connect();

            //await busDevice.PropertyRead(0, 11);

            //var x = await busDevice.PropertyDescriptionRead(0, 11);

            //await busDevice.Disconnect();

            //await Task.Delay(1000);
            //await _conn.Disconnect();

            //await Task.Delay(3000);
            //SearchForKNXDevices();
        }

        private async void SearchForKNXDevices()
        {
            //List<DeviceScanResult> responses = new();

            //foreach (var response in _searchResponses)
            //{
            //    if (response.Endpoint == null)
            //        continue;
            //    KnxIpTunneling conn = new KnxIpTunneling(response.Endpoint);
            //    await conn.Connect();
            //    BusCommon bus = new BusCommon(conn);

            //    conn.OnTunnelResponse += (IMessageResponse message) =>
            //    {
            //        if (message is MsgReadSerialNumberRes resp)
            //        {
            //            Debug.WriteLine("Found OpenKNX Device with Serial: " + BitConverter.ToString(resp.TestResult.Skip(2).ToArray()).Replace("-", ""));
            //            responses.Add(new(resp, response.Endpoint));
            //        }
            //    };

            //    var x = await bus.ReadSerialNumberByManufacturer(0x00FA); // OpenKNX

            //    await Task.Delay(5000);

            //    await conn.Disconnect();
            //    break;
            //}


            //foreach (var tunnelEndpoint in responses.GroupBy(r => r.EndPoint))
            //{
            //    KnxIpTunneling conn = new KnxIpTunneling(tunnelEndpoint.Key);
            //    await conn.Connect();

            //    foreach (var responseDevice in tunnelEndpoint)
            //    {
            //        if (responseDevice == null || responseDevice.SerialNumberResponse.SourceAddress == null)
            //            continue;

            //        UnicastAddress deviceAddress = (UnicastAddress)responseDevice.SerialNumberResponse.SourceAddress;
            //        bool addressWasUnset = false;
            //        if (responseDevice.SerialNumberResponse.SourceAddress.AsUInt16() == 0xFFFF)
            //        {
            //            addressWasUnset = true;
            //            deviceAddress = UnicastAddress.FromString("15.14.254");
            //            await SetDeviceAddress(conn, responseDevice.SerialNumberResponse.TestResult.Skip(2).ToArray(), deviceAddress);
            //        }

            //        BusDevice busDevice = new BusDevice(deviceAddress, conn);

            //        DeviceModel deviceModel = new();
            //        deviceModel.Name = "TP Device";
            //        deviceModel.Address = responseDevice.SerialNumberResponse.SourceAddress.ToString();
            //        deviceModel.SerialNumber = $"{BitConverter.ToString(responseDevice.SerialNumberResponse.TestResult.Skip(2).Take(2).ToArray()).Replace("-", "")}:{BitConverter.ToString(responseDevice.SerialNumberResponse.TestResult.Skip(4).ToArray()).Replace("-", "")}";


            //        await busDevice.Connect();

            //        var y = await busDevice.PropertyRead(0, 78);
            //        if (y.Length > 0)
            //        {
            //            Debug.WriteLine("AppId: " + BitConverter.ToString(y).Replace("-", ""));
            //            deviceModel.AppId = $"${BitConverter.ToString(y.Skip(2).Take(2).ToArray()).Replace("-", "")}";
            //        }

            //        y = await busDevice.PropertyRead(0, 25);
            //        if (y.Length > 0)
            //        {
            //            Debug.WriteLine("Version: " + BitConverter.ToString(y).Replace("-", ""));
            //            short version = BitConverter.ToInt16(y.Reverse().ToArray());
            //            int major = (version >> 6) & 0x1F;
            //            int minor = version & 0x3F;
            //            int revision = (version >> 11) & 0x1F;
            //            deviceModel.FirmwareVersion = new(major, minor, revision);
            //            deviceModel.FirmwareVersionString = deviceModel.FirmwareVersion.ToString();
            //        }

            //        UpdateDeviceInfo(deviceModel);
            //        DeviceModels.Add(deviceModel);

            //        await busDevice.Disconnect();

            //        if (addressWasUnset)
            //        {
            //            await SetDeviceAddress(conn, responseDevice.SerialNumberResponse.TestResult.Skip(2).ToArray(), UnicastAddress.FromString("15.15.255"));
            //        }

            //    }

            //    await conn.Disconnect();
            //}
        }

        //private async Task SetDeviceAddress(KnxIpTunneling conn, byte[] serial, UnicastAddress newAddr)
        //{
        //    MsgIndividualAddressSerialWriteReq writeReq = new MsgIndividualAddressSerialWriteReq(newAddr, serial);
        //    await conn.Send(writeReq);
        //}

        //private void Search_OnSearchResponse(Kaenx.Konnect.Messages.Response.MsgSearchRes message, System.Net.NetworkInformation.NetworkInterface? netInterface, int netIndex)
        //{
        //    Debug.WriteLine("Found KNX IP Interface: " + message.FriendlyName);
        //    if (!message.SupportedServiceFamilies.Any(sf => sf.ServiceFamilyType == ServiceFamilyTypes.Tunneling) || _searchResponses.Any(r => r.Endpoint?.Address == message.Endpoint?.Address))
        //        return;
        //    _searchResponses.Add(message);
        //}

        private void DeviceModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUpdatesAvailable();
        }

        private void DeviceFound(object? sender, ServiceAnnouncementEventArgs e)
        {
            DeviceModel model = new DeviceModel()
            {
                Name = e.Announcement.Hostname,
                IP = e.Announcement.Addresses.First().ToString(),
                IsIP = true
            };

            foreach (string txt in e.Announcement.Txt)
            {
                if (!txt.Contains("="))
                    continue;

                string key = txt.Split("=")[0];
                string value = txt.Split("=")[1];

                switch (key)
                {
                    case "configured":
                        model.Configured = value == "1";
                        break;
                    case "ota":
                        model.Port = int.Parse(value);
                        break;
                    case "address":
                        model.Address = value;
                        break;
                    case "firmware":
                        model.AppId = value;
                        break;
                    case "version":
                        model.FirmwareVersion = new SemanticVersion(value);
                        model.FirmwareVersionString = value;
                        break;
                    case "serial":
                        model.SerialNumber = value;
                        break;
                }
            }

            if (DeviceModels.Any(d => d.SerialNumber == model.SerialNumber))
                return;

            if (model.Port == 2040)
                model.Architecture = Lib.Data.ArchitectureType.RP2040;
            else if (model.Port == 3232)
                model.Architecture = Lib.Data.ArchitectureType.ESP32;
            else
                MainViewModel.Instanz.ShowError(Properties.Resources.UnknownPort, Properties.Resources.UnknownPortMessage + model.Port);

            UpdateDeviceInfo(model);
            DeviceModels.Add(model);
        }

        private void UpdateDeviceInfo(DeviceModel model)
        {
            ApplicationModel? app = FirmwareManagerViewModel.Instanz.Applications.FirstOrDefault(a => a.AppId == model.AppId);
            if (app != null)
            {
                model.FirmwareName = app.Name;
                model.UpdateVersion = GetNewstVersion(app, model.FirmwareVersion);
                if (model.UpdateVersion != null)
                    model.UpdateVersionString = model.UpdateVersion.ToString();
            }
            else
            {
                RepositoryMapping? map = GitHubAccess.GetRepoMappingByAppId(model.AppId);
                if (map != null)
                    model.FirmwareName = map.Name;
            }

            if (AppToImage.ContainsKey(model.AppId))
                model.ImageUrl = AppToImage[model.AppId];
            else
                model.ImageUrl = "http://icon-library.com/images/placeholder-icon/placeholder-icon-15.jpg";

            UpdateUpdatesAvailable();
        }

        private SemanticVersion? GetNewstVersion(ApplicationModel app, SemanticVersion? currentVersion)
        {
            if (currentVersion == null)
                return null;
            SemanticVersion? newest = null;
            foreach (ReleaseModel release in app.Releases)
            {
                SemanticVersion ver = new SemanticVersion(release.Version);
                if (newest == null || ver > newest)
                    newest = ver;
            }
            if (newest != null && newest > currentVersion)
                return newest;
            return null;
        }

        private void UpdateUpdatesAvailable()
        {
            UpdatesAvailable = DeviceModels.Count(d => d.UpdateVersion != null);
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
