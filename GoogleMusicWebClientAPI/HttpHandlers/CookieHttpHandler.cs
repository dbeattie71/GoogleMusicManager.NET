﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI.HttpHandlers
{
    class CookieHttpHandler : DelegatingHandler, IGoogleCookieManager
    {
        public CookieHttpHandler()
        {
            cookieContainer = new CookieContainer();
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("Cookie", this.GetCookies());

            var response = await base.SendAsync(request, cancellationToken);

            this.HandleResponse(response);

            return response;
        }

        public static String URI = "https://play.google.com/music/";
        private CookieContainer cookieContainer;

        private IEnumerable<Cookie> Cookies
        {
            get
            {
                return GetCookiesList();
            }
        }

        public bool HandleResponse(HttpResponseMessage msg)
        {
            bool cookiesChanged = false;
            IEnumerable<String> cookies;
            if (msg.Headers.TryGetValues("Set-Cookie", out cookies))
            {
                foreach (String cookie in cookies)
                {
                    cookieContainer.SetCookies(new Uri(URI), cookie);
                    cookiesChanged = true;
                }
            }

            return cookiesChanged;
        }

        public void SetCookiesFromList(List<Cookie> cookies)
        {
            if (cookies == null) return;

            foreach (Cookie c in cookies)
                cookieContainer.Add(new Uri(URI), c);
        }

        public String GetCookies()
        {
            return cookieContainer.GetCookieHeader(new Uri(URI));
        }

        public List<Cookie> GetCookiesList()
        {
            List<Cookie> cookies = new List<Cookie>();
            foreach (Cookie cookie in cookieContainer.GetCookies(new Uri(URI)))
            {
                cookies.Add(cookie);
            }

            return cookies;
        }

        public String GetXtCookie()
        {
            // Get the last one
            String xt = "";
            foreach (Cookie cook in this.GetCookiesList())
                if (cook.Name.Equals("xt"))
                    xt = cook.Value;

            return xt;
        }

    }
}
