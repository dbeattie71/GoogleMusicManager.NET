using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.DeviceId
{
    class MacAddressDeviceId : IDeviceId
    {
        public string GetDeviceId()
        {
            return this.GetMACAddress();
        }

        private string GetMACAddress()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().Where(p => p.NetworkInterfaceType != NetworkInterfaceType.Loopback && p.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
            {
                var macaddressbytes = nic.GetPhysicalAddress().GetAddressBytes();
                if (macaddressbytes.Length > 0)
                {
                    macaddressbytes[macaddressbytes.Length - 1] = (byte)(macaddressbytes[macaddressbytes.Length - 1] + 0x1);
                    return BitConverter.ToString(macaddressbytes).Replace("-", ":");
                }
            }
            return null;
        }
    }
}
