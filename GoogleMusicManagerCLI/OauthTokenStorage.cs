using GoogleMusicManagerAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicAPICLI
{
    class OauthTokenStorage : IOauthTokenStorage
    {
        private readonly string storageFile = "oauth2.json";

        public Oauth2Token GetOauthToken()
        {
            if (File.Exists(storageFile))
            {
                var oauthString = File.ReadAllText(storageFile);
                var oauth = JsonConvert.DeserializeObject<Oauth2Token>(oauthString);
                return oauth;
            }
            else
            {
                return null;
            }
        }

        public void SaveOauthToken(Oauth2Token result)
        {
            var oauthString = JsonConvert.SerializeObject(result);
            File.WriteAllText(storageFile, oauthString);
        }

        public string GetOauthKeyFromUser(string authUrl)
        {
            Console.WriteLine(authUrl);
            Console.WriteLine("Input response: ");

            var response = Console.ReadLine();
            return response;
        }

        public TimeSpan GetOauthTokenAge()
        {
            if (File.Exists(storageFile) == false) return new TimeSpan(24,0,0);
            var lastsaved = DateTime.UtcNow - File.GetLastWriteTimeUtc(storageFile);
            return lastsaved;
        }
    }
}
