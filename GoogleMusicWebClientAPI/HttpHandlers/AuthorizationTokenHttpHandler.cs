using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GoogleMusicWebClientAPI.HttpHandlers
{
    class AuthorizationTokenHttpHandler : DelegatingHandler
    {
        public string AuthorizationToken { get; set; }
        public DateTime AuthTokenIssueDate { get; set; }

        public AuthorizationTokenHttpHandler()
        {
            this.AuthorizationToken = string.Empty;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            this.SetAuthHeader(request);

            var response = await base.SendAsync(request, cancellationToken);

            this.CheckForUpdatedAuth(response);

            return response;
        }

        /// <summary>
        /// Set the auth token from the login data
        /// </summary>
        /// <param name="loginData"></param>
        public void SetAuthToken(String loginData)
        {
            string CountTemplate = @"Auth=(?<AUTH>(.*?))$";
            var CountRegex = new Regex(CountTemplate, RegexOptions.IgnoreCase);
            string auth = CountRegex.Match(loginData).Groups["AUTH"].ToString();
            this.AuthorizationToken = auth;

            this.AuthTokenIssueDate = DateTime.Now;
        }

        /// <summary>
        /// Sets Google's auth header
        /// </summary>
        private void SetAuthHeader(HttpRequestMessage request)
        {
            if (!this.AuthorizationToken.Equals(String.Empty))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(String.Format("GoogleLogin auth={0}", this.AuthorizationToken));
            }
        }

        private bool CheckForUpdatedAuth(HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                if (header.Key.Equals("Update-Client-Auth"))
                {
                    foreach (var v in header.Value)
                    {
                        this.AuthorizationToken = v;
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
