using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes
{
    internal class DeviceScanResult
    {
        // TODO
        public object SerialNumberResponse { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public DeviceScanResult(object serialNumberResponse, IPEndPoint endPoint)
        {
            SerialNumberResponse = serialNumberResponse;
            EndPoint = endPoint;
        }
    }
}
