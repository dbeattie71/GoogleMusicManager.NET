using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.HTTPHeaders
{
    public class DeviceIDHeaderBuilder : IHttpHeaderBuilder
    {
        private IDeviceId deviceId;
        public DeviceIDHeaderBuilder(IDeviceId clientId)
        {
            this.deviceId = clientId;
        }
        public void AssignHeaders(HttpRequestHeaders headers)
        {
            headers.Add("X-Device-ID", deviceId.GetDeviceId());
        }
    }
}
