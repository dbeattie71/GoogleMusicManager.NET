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
    public class GoogleOauth2HTTP
    {
        private readonly static string MUSIC_MANAGER_USER_AGENT = "Music Manager (1, 0, 55, 7425 HTTPS - Windows)";

        /// <summary>
        /// The HttpClient
        /// </summary>
        private HttpClient client;

        private string RejectedReason { get; set; }

        private IOauthTokenStorage tokenStorage { get; set; }

        public GoogleOauth2HTTP(IOauthTokenStorage tokenStorage)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                UseProxy = false,
            };

            var progressHandler = new ProgressMessageHandler();
            progressHandler.HttpSendProgress += progressHandler_HttpSendProgress;
            progressHandler.HttpReceiveProgress += progressHandler_HttpReceiveProgress;

            client = HttpClientFactory.Create(
                handler,
                progressHandler
                );
            client.Timeout = new TimeSpan(0, 10, 0);
            this.tokenStorage = tokenStorage;
        }

        void progressHandler_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
            Debug.WriteLine("Receive: " + e.ProgressPercentage.ToString());

            //Console.WriteLine("Receive: " + e.ProgressPercentage.ToString());
        }

        void progressHandler_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            Debug.WriteLine("Send: " + e.ProgressPercentage.ToString());

            //Console.WriteLine("Send: " + e.ProgressPercentage.ToString());
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
        public async Task<byte[]> Request(HttpMethod method, Uri address, byte[] bytes)
        {
            using (var requestMessage = new HttpRequestMessage(method, address))
            {
                var token = tokenStorage.GetOauthToken();
                requestMessage.Headers.Add("Authorization", token.token_type + " " + token.access_token);
                requestMessage.Headers.Add("User-agent", MUSIC_MANAGER_USER_AGENT);
                requestMessage.Content = new ByteArrayContent(bytes);
                
                var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                responseMessage.EnsureSuccessStatusCode();

                CheckForRejection(responseMessage);

                byte[] retnData = await responseMessage.Content.ReadAsByteArrayAsync();
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
