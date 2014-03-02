using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace GoogleMusicWebClientAPI
{
    interface IGoogleHTTP
    {
        string AuthroizationToken { get; set; }
        DateTime AuthTokenIssueDate { get; set; }
        IGoogleCookieManager CookieManager { get; set; }
        event EventHandler CookiesChanged;
        Task<string> GET(Uri address);
        Task<T> GET<T>(Uri address);
        string GetXtCookie();
        HttpStatusCode LastStatusCode { get; set; }
        Task<string> POST(Uri address, HttpContent content = null);
        Task<T> POST<T>(Uri address, HttpContent content = null);
        string RejectedReason { get; set; }
        void SetAuthToken(string loginData);
    }
}
