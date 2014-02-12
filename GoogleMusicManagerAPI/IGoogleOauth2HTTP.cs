using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace GoogleMusicManagerAPI
{
    public interface IGoogleOauth2HTTP
    {
        Task<Stream> Request(HttpMethod method, Uri address, Stream stream);
    }
}
