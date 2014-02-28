using GoogleMusicManagerAPI;
using GoogleMusicManagerAPI.HTTPHeaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicDownloaderCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var downloader = new DownloadProcess(
                    new OauthTokenStorage(options.OauthFile),
                    new DownloadProcessObserver()
                    );

                var task = downloader.DoDownload(options.ArtistFilter, options.AlbumFilter, options.TrackFilter);

                task.Wait();
                var result = task.Result;
            }
        }




    }
}
