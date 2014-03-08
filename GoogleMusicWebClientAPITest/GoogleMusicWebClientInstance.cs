using GoogleMusicWebClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPITest
{
    class GoogleMusicWebClientInstance
    {
        private static IGoogleMusicWebClient instance { get; set; }

        public static IGoogleMusicWebClient GetInstance()
        {
            if (instance == null)
            {
                if (File.Exists("googleauth.json") == false)
                {
                    var errorMessage = @"
{
	""UserName"":""your google username"",
	""Password"":""your google password""
}
";
                    throw new ApplicationException("Create a file named googleauth.json with the following contents: \n" + errorMessage);
                }

                var jsonAuth = File.ReadAllText("googleauth.json");
                var googleAuth = JsonConvert.DeserializeObject<GoogleAuth>(jsonAuth);

                var api = new GoogleMusicWebClient(new SessionStorage());

                var loginTask = api.Login(googleAuth.UserName, googleAuth.Password);
                loginTask.Wait();
                var result = loginTask.Result;

                if (result == false)
                {
                    throw new ApplicationException("Login failed");
                }

                instance = api;
            }
            return instance;
        }

    }
}
