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
        /// <summary>
        /// The HttpClient
        /// </summary>
        private HttpClient client;

        private string RejectedReason { get; set; }

        private IEnumerable<IHttpHeaderBuilder> headerBuilders { get; set; }

        public GoogleOauth2HTTP(IEnumerable<IHttpHeaderBuilder> headerBuilders, params DelegatingHandler[] handlers)
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
            this.headerBuilders = headerBuilders;
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

        public async Task<Stream> Request(HttpMethod method, Uri address, Stream stream)
        {
            var content = stream == null ? (StreamContent)null : new StreamContent(stream);
            return await this.Request(method, address, content);
        }

        public async Task<Stream> Request(HttpMethod method, Uri address, HttpContent content)
        {
            using (var requestMessage = new HttpRequestMessage(method, address))
            {
                if (headerBuilders != null)
                {
                    var headers = requestMessage.Headers;
                    foreach (var headerBuilder in headerBuilders)
                    {
                        headerBuilder.AssignHeaders(headers);
                    }
                }
                requestMessage.Content = content;

                var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                responseMessage.EnsureSuccessStatusCode();

                CheckForRejection(responseMessage);

                var retnData = await responseMessage.Content.ReadAsStreamAsync();
                return retnData;
            }
        }

        public async Task<Stream> Request(HttpMethod method, Uri address)
        {
            return await this.Request(method, address, (Stream)null);
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
