using GoogleMusicManagerAPI;
using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicDownloaderCLI
{
    class DownloadProcessObserver : IDownloadProcessObserver
    {
        private ITrackMetadata track;
        private string operation = string.Empty;
        private int progress = 0;
        private bool enableProgress = false;

        private string lastArtist = string.Empty;
        private string lastAlbum = string.Empty;

        private string GetProgress()
        {
            var result = "\r";

            result += string.Format(
                "{0, -4}", track.TrackNumber.ToString()
                );

            if (track.Title.Length > 35)
            {
                result += track.Title.Substring(0, 35);
            }
            else
            {
                result += track.Title.PadRight(35);
            }

            var length = track.Duration;
            result += length.ToString(@"mm\:ss");
            result += " ";

            result += string.Format("{0, -4}", track.AudioBitrate);
            result += " ";

            result += this.operation;
            if (enableProgress)
            {
                result += " ";
                result += this.progress.ToString() + "%";
            }
            result = result.PadRight(Console.WindowWidth);

            return result;
        }

        private void SetProgress(string operation, bool enableProgress)
        {
            if (enableProgress != this.enableProgress)
            {
                this.progress = 0;
                this.enableProgress = enableProgress;
            }
            this.operation = operation;
            Console.Write(this.GetProgress());
        }


        public void BeginDownloadTracks(IEnumerable<ITrackMetadata> trackMetadata)
        {
            Console.WriteLine("Downloading " + trackMetadata.Count() + " tracks");
        }

        public void DownloadTrackExists(ITrackMetadata trackMetadata)
        {
            this.SetProgress("Exists", false);
            Console.WriteLine();
        }

        public void BeginDownloadTrack(ITrackMetadata trackMetadata)
        {
            if (this.lastArtist != trackMetadata.Artist)
            {
                this.lastArtist = trackMetadata.Artist;
                Console.Write("Artist: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(trackMetadata.Artist);
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
            this.SetProgress(string.Empty, true);
        }

        public void EndDownloadTrack(ITrackMetadata trackMetadata)
        {
            this.SetProgress("Complete", false);
            Console.WriteLine();
        }


        public void ReceiveProgress(int p)
        {
            if (enableProgress)
            {
                this.progress = p;
                Console.Write(this.GetProgress());
            }
        }
    }
}
