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
using Byteopia.Helpers;


namespace GoogleMusicWebClientAPI
{
    /// <summary>
    /// Wraps an HttpClient for use with Google requests
    /// </summary>
    ///
    public class GoogleHTTP : IGoogleHTTP
    {
        /// <summary>
        /// The HttpClient
        /// </summary>
        private HttpClient client;

        public HttpStatusCode LastStatusCode { get; set; }

        public string RejectedReason { get; set; }

        public GoogleHTTP(params DelegatingHandler[] handlers)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false
            };

            client = HttpClientFactory.Create(
                handler,
                handlers
                );
        }

        /// <summary>
        /// Generic POST method that deserializes its result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<T> POST<T>(Uri address, HttpContent content = null)
        {
            var response = await POST(address, content);
            return JSON.Deserialize<T>(response);
        }

        /// <summary>
        /// Generic GET method that deserializes its result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<T> GET<T>(Uri address)
        {
            var response = await GET(address);
            return JSON.Deserialize<T>(response);
        }

        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="address">end point</param>
        /// <param name="content">content</param>
        /// <returns></returns>
        public async Task<String> POST(Uri address, HttpContent content = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, address);
            requestMessage.Content = content;

            var responseMessage = await client.SendAsync(requestMessage);

            LastStatusCode = responseMessage.StatusCode;

            CheckForRejection(responseMessage);

            var retnData = await responseMessage.Content.ReadAsStringAsync();

            return retnData;
        }


        /// <summary>
        /// GET request
        /// </summary>
        /// <param name="address">endpoint</param>
        /// <returns></returns>
        public async Task<String> GET(Uri address)
        {

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, address);
            var responseMessage = await client.SendAsync(requestMessage);

            LastStatusCode = responseMessage.StatusCode;

            responseMessage.EnsureSuccessStatusCode();

            CheckForRejection(responseMessage);

            var retnData = await responseMessage.Content.ReadAsStringAsync();

            return retnData;
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


        ///// <summary>
        ///// Append xt cookie value to each request
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <returns></returns>
        //private Uri BuildGoogleRequest(Uri uri)
        //{
        //    return uri;

        //    String xt = this.CookieManager.GetXtCookie();
        //    if (xt.Equals(String.Empty))
        //        return uri;

        //    if (uri.ToString().Contains("songid"))
        //        return uri;

        //    if (uri.ToString().StartsWith("https://play.google.com/music/listen"))
        //        return uri;

        //    if (uri.ToString().StartsWith("https://www.google.com/accounts/Logout"))
        //        return uri;

        //    return new Uri(uri.OriginalString + String.Format("?u=0&xt={0}", xt));
        //}


    }
}