using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
namespace GoogleMusicWebClientAPI
{
    public interface IGoogleCookieManager
    {
        string GetCookies();
        List<Cookie> GetCookiesList();
        bool HandleResponse(HttpResponseMessage msg);
        void SetCookiesFromList(List<Cookie> cookies);
    }
}
