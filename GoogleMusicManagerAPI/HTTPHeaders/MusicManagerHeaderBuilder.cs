using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.HTTPHeaders
{
    public class MusicManagerHeaderBuilder : GoogleMusicManagerAPI.IHttpHeaderBuilder
    {
        private readonly static string MUSIC_MANAGER_USER_AGENT = "Music Manager (1, 0, 55, 7425 HTTPS - Windows)";

        public MusicManagerHeaderBuilder()
        {
        }

        public void AssignHeaders(System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            headers.Add("User-agent", MUSIC_MANAGER_USER_AGENT);
        }

    }
}
