using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI
{
    public interface IOauthTokenStorage
    {
        string GetOauthKeyFromUser(string authUrl);
        Oauth2Token GetOauthToken();
        void SaveOauthToken(Oauth2Token result);
        TimeSpan GetOauthTokenAge();
    }
}
