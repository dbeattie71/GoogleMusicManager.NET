using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.HTTPHeaders
{
    public class Oauth2HeaderBuilder : IHttpHeaderBuilder
    {
        private IOauthTokenStorage tokenStorage { get; set; }

        public Oauth2HeaderBuilder(IOauthTokenStorage storage)
        {
            this.tokenStorage = storage;
        }

        public void AssignHeaders(System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            var token = tokenStorage.GetOauthToken();
            headers.Add("Authorization", token.token_type + " " + token.access_token);
        }


    }
}
