using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI
{
    public class GoogleHTTP
    {
        private HttpClient client;

        public string RejectedReason { get; set; }

        public GoogleHTTP()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                UseProxy = false,
            };

            client = new HttpClient(handler);
        }

        public async Task<T> POST<T>(Uri address, HttpContent content = null)
        {
            var response = await POST(address, content);
            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<String> POST(Uri address, HttpContent content = null)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, address))
            {
                requestMessage.Content = content;
                var responseMessage = await client.SendAsync(requestMessage);

                responseMessage.EnsureSuccessStatusCode();

                CheckForRejection(responseMessage);

                var retnData = await responseMessage.Content.ReadAsStringAsync();

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