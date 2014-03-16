using GoogleMusicManagerAPI;
using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerCLI
{
    class UploadProcessObserver : IUploadProcessObserver
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

            var title = string.IsNullOrEmpty(track.Title) ? "<untitled>" : track.Title;

            if (title.Length > 35)
            {
                result += title.Substring(0, 35);
            }
            else
            {
                result += title.PadRight(35);
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

        public void BeginTrack(ITrackMetadata track)
        {
            if (this.lastArtist != track.Artist)
            {
                this.lastArtist = track.Artist;
                Console.Write("Artist: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(track.Artist);
                Console.ResetColor();
            }
            if (this.lastAlbum != track.Album)
            {
                this.lastAlbum = track.Album;
                Console.Write("Album: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(track.Album);
                Console.ResetColor();
            }

            this.track = track;
            this.SetProgress(string.Empty, false);
        }

        public void BeginMetadata(ITrackMetadata track)
        {
            this.SetProgress("Sending metadata...", false);
        }

        public void MetadataMatch(ITrackMetadata track)
        {
            this.SetProgress("Metadata matched...", false);
        }

        public void MetadataMatchRetry(ITrackMetadata track, int matchRetryCount)
        {
            this.SetProgress("Metadata retry " + matchRetryCount, false);
        }

        public void MetadataNoMatch(ITrackMetadata track)
        {
            this.SetProgress("Metadata no match", false);
        }

        public void BeginUploadSample(ITrackMetadata track)
        {
            this.SetProgress("Uploading sample", true);
        }

        public void EndUploadSample(ITrackMetadata track, string responseCode)
        {
            this.SetProgress("Sample result: " + responseCode.ToString().ToLower().Replace("_", " "), false);
        }


        public void BeginUploadTrack(ITrackMetadata track)
        {
            this.SetProgress("Uploading track", true);
        }

        public void EndUploadTrack(ITrackMetadata track, string status, string serverFileReference)
        {
            this.SetProgress("Upload track result: " + status.ToString().ToLower().Replace("_", " "), false);
        }

        public void EndTrack(ITrackMetadata track)
        {
            Console.WriteLine();
        }


        public void SendProgress(int p)
        {
            if (enableProgress)
            {
                this.progress = p;
                Console.Write(this.GetProgress());
            }
        }


        public void BeginSessionRequest(ITrackMetadata track)
        {
            this.SetProgress("Uploading session request...", false);
        }

        public void RetrySessionRequest(ITrackMetadata track, int retryCount)
        {
            this.SetProgress("Retry session request " + retryCount, false);
        }

        public void EndSessionRequest(ITrackMetadata track)
        {
            this.SetProgress("Received session", false);
        }
    }
}
