using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kaenx.Konnect.Addresses;

namespace OpenKNX.Toolbox.Models
{
    public class ConnectionModel
    {
        public string SerialNumber { get; set; } = "unknown";
        public string FriendlyName { get; set; } = "unknown";
        public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 0);
        public UnicastAddress? UnicastAddress { get; set; } = null;
        public int Counter { get; set; } = 2;
        public bool IsManuallyAdded { get; set; } = false;
        public bool IsTunnel { get; set; } = false;
        public bool IsTCP { get; set; } = false;

        public override string ToString()
        {
            string prefix = $"{(IsTunnel ? "Tunnel " : "Routing")} [{(IsTCP ? "TCP" : "UDP")}]";
            return $"{prefix} {FriendlyName} {UnicastAddress?.ToString() ?? "??.??.???"} ({EndPoint.Address}:{EndPoint.Port}) {(IsManuallyAdded ? "[MAN]" : "")}";
        }
    }
}
