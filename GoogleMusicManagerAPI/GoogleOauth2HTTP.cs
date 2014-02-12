using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI
{
    public class GoogleOauth2HTTP : IGoogleOauth2HTTP
    {
        private readonly static string MUSIC_MANAGER_USER_AGENT = "Music Manager (1, 0, 55, 7425 HTTPS - Windows)";

        /// <summary>
        /// The HttpClient
        /// </summary>
        private HttpClient client;

        private string RejectedReason { get; set; }

        private IOauthTokenStorage tokenStorage { get; set; }

        public GoogleOauth2HTTP(IOauthTokenStorage tokenStorage, params DelegatingHandler[] handlers)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                UseProxy = false,
            };

            client = HttpClientFactory.Create(
                handler,
                handlers
                );
            client.Timeout = new TimeSpan(0, 10, 0);
            this.tokenStorage = tokenStorage;
        }

        ///// <summary>
        ///// Generic POST method that deserializes its result
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="address"></param>
        ///// <param name="content"></param>
        ///// <returns></returns>
        //public async Task<T> POST<T>(Uri address, HttpContent content = null)
        //{
        //    var response = await POST(address, content);
        //    return JSON.Deserialize<T>(response);
        //}

        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="address">end point</param>
        /// <param name="content">content</param>
        /// <returns></returns>
        public async Task<Stream> Request(HttpMethod method, Uri address, Stream stream)
        {
            using (var requestMessage = new HttpRequestMessage(method, address))
            {
                var token = tokenStorage.GetOauthToken();
                requestMessage.Headers.Add("Authorization", token.token_type + " " + token.access_token);
                requestMessage.Headers.Add("User-agent", MUSIC_MANAGER_USER_AGENT);

                requestMessage.Content = new StreamContent(stream);

                var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                responseMessage.EnsureSuccessStatusCode();

                CheckForRejection(responseMessage);

                var retnData = await responseMessage.Content.ReadAsStreamAsync();
                return retnData;
            }
        }

        private void CheckForRejection(HttpResponseMessage responseMessage)
        {
            this.RejectedReason = String.Empty;

            foreach (var header in responseMessage.Headers)
            {
                if (header.Key.Equals("X-Rejected-Reason"))
                {
                    foreach (var v in header.Value)
                    {
                        this.RejectedReason = v;
                        break;
                    }
                }
            }
        }

    }
}
