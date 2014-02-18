using System;
using System.Net.Http.Headers;
namespace GoogleMusicManagerAPI
{
    public interface IHttpHeaderBuilder
    {
        void AssignHeaders(HttpRequestHeaders headers);
    }
}
