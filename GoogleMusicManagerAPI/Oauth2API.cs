using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GoogleMusicManagerAPI
{
    public class Oauth2API
    {
        GoogleHTTP client;
        IOauthTokenStorage tokenStorage;

        const string clientId = "652850857958.apps.googleusercontent.com";
        const string clientSecret = "ji1rklciNp2bfsFJnEH_i6al";
        const string scope = "https://www.googleapis.com/auth/musicmanager";
        const string redirectUri = "urn:ietf:wg:oauth:2.0:oob";

        public Oauth2API(IOauthTokenStorage oauthTokenStorage)
        {
            client = new GoogleHTTP();
            this.tokenStorage = oauthTokenStorage;
        }
        public async Task<Oauth2Token> Authenticate()
        {
            var oauth = tokenStorage.GetOauthToken();

            if (oauth == null)
            {
                await DoAuth();
            }
            else
            {
                var age = tokenStorage.GetOauthTokenAge();
                if (age.TotalSeconds > oauth.expires_in)
                {
                    await DoReauth(oauth);
                }
                else
                {
                    Debug.WriteLine("Reusing token");
                }
            }

            ScheduleReauth();

            return oauth;
        }

        private void ScheduleReauth()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var oauth2 = tokenStorage.GetOauthToken();
                    var age2 = tokenStorage.GetOauthTokenAge();
                    var timeToReauth = (int)(oauth2.expires_in - age2.TotalSeconds) - 600;
                    if (timeToReauth < 0) timeToReauth = 0;
                    Debug.WriteLine("Auto reauth in " + timeToReauth + "s");
                    await Task.Delay(1000 * timeToReauth);
                    await this.DoReauth(oauth2);
                    Debug.WriteLine("Auto reauth complete");
                }
            });
        }

        private async Task DoAuth()
        {
            var authUrl = this.GetOauth2AuthURI();
            var response = tokenStorage.GetOauthKeyFromUser(authUrl);
            var oauthtask = await this.Oauth2Exchange(response);
            tokenStorage.SaveOauthToken(oauthtask);
        }

        private async Task DoReauth(Oauth2Token oauth)
        {
            var refreshToken = oauth.refresh_token;
            var result = await this.Oauth2Refresh(refreshToken);
            oauth.access_token = result.access_token;
            oauth.token_type = result.token_type;
            oauth.expires_in = result.expires_in;
            tokenStorage.SaveOauthToken(oauth);
        }


        public string GetOauth2AuthURI()
        {
            var url = "https://accounts.google.com/o/oauth2/auth?" +
                "scope=" + HttpUtility.UrlEncode(scope) + "&" +
                "redirect_uri=" + HttpUtility.UrlEncode(redirectUri) + "&" +
                "response_type=code&" +
                "client_id=" + clientId + "&" +
                "access_type=offline";
            return url;
        }
        
        private async Task<Oauth2Token> Oauth2Exchange(string code)
        {
            //POST /o/oauth2/token HTTP/1.1
            //Host: accounts.google.com
            //Content-Type: application/x-www-form-urlencoded

            //code=4/v6xr77ewYqhvHSyW6UJ1w7jKwAzu&
            //client_id=8819981768.apps.googleusercontent.com&
            //client_secret={client_secret}&
            //redirect_uri=https://oauth2-login-demo.appspot.com/code&
            //grant_type=authorization_code

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            });

            var loginData = await client.POST<Oauth2Token>(new Uri("https://accounts.google.com/o/oauth2/token"), content);
            return loginData;
        }

        private async Task<Oauth2Token> Oauth2Refresh(string refresh_token)
        {
            //POST /o/oauth2/token HTTP/1.1
            //Host: accounts.google.com
            //Content-Type: application/x-www-form-urlencoded

            //client_id=8819981768.apps.googleusercontent.com&
            //client_secret={client_secret}&
            //refresh_token=1/6BMfW9j53gdGImsiyUH5kU5RsR4zwI9lUVX-tqf8JXQ&
            //grant_type=refresh_token

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", refresh_token),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
            });

            var loginData = await client.POST<Oauth2Token>(new Uri("https://accounts.google.com/o/oauth2/token"), content);
            return loginData;
        }
    }
}
