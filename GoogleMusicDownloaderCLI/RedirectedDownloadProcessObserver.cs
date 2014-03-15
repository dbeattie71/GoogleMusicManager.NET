using GoogleMusicManagerAPI;
using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicDownloaderCLI
{
    class RedirectedDownloadProcessObserver : IDownloadProcessObserver
    {
        private ITrackMetadata track;
        private string operation = string.Empty;
        private string lastArtist = string.Empty;
        private string lastAlbum = string.Empty;

        private string GetProgress()
        {
            var result = "\r";

            result += string.Format(
                "{0, -4}", track.TrackNumber.ToString()
                );

            if (track.Title.Length > 80)
            {
                result += track.Title.Substring(0, 80);
            }
            else
            {
                result += track.Title.PadRight(80);
            }

            result += this.operation;
            result = result.PadRight(Console.WindowWidth);

            return result;
        }

        private void SetProgress(string operation, bool enableProgress)
        {
            this.operation = operation;
            Console.Write(this.GetProgress());
        }


        public void BeginDownloadTracks(IEnumerable<ITrackMetadata> trackMetadata)
        {
        }

        public void DownloadTrackExists(ITrackMetadata trackMetadata)
        {
            this.SetProgress("Exists", false);
            Console.WriteLine();
        }

        public void BeginDownloadTrack(ITrackMetadata trackMetadata)
        {
            var preferredArtistName = string.IsNullOrEmpty(trackMetadata.AlbumArtist) ? trackMetadata.Artist : trackMetadata.AlbumArtist;

            if (this.lastArtist != preferredArtistName)
            {
                this.lastArtist = preferredArtistName;
                Console.Write("Artist: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(preferredArtistName);
                Console.ResetColor();
            }
            if (this.lastAlbum != trackMetadata.Album)
            {
                this.lastAlbum = trackMetadata.Album;
                Console.Write("Album: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(trackMetadata.Album);
                Console.ResetColor();
            }

            this.track = trackMetadata;
        }

        public void EndDownloadTrack(ITrackMetadata trackMetadata)
        {
            this.SetProgress("Complete", false);
            Console.WriteLine();
        }


        public void ReceiveProgress(int p)
        {
        }
    }
}
