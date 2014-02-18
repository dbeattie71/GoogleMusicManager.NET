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
        public void AssignHeaders(HttpRequestHeaders headers)
        {
            headers.Add("X-Device-ID", "00:22:15:15:2A:65");
        }
    }
}
