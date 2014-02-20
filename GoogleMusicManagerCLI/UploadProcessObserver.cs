using GoogleMusicManagerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerCLI
{
    class UploadProcessObserver : IUploadProcessObserver
    {
        private Track track;
        private string operation = string.Empty;
        private int progress = 0;
        private bool enableProgress = false;

        private string lastArtist = string.Empty;
        private string lastAlbum = string.Empty;

        private string GetProgress()
        {
            var result = "\r";

            result += string.Format(
                "{0, -4}", track.track_number.ToString()
                );

            if (track.title.Length > 35)
            {
                result += track.title.Substring(0, 35);
            }
            else
            {
                result += track.title.PadRight(35);
            }

            var length = new TimeSpan(0, 0, 0, 0, (int)track.duration_millis);
            result += length.ToString(@"mm\:ss");
            result += " ";

            result += string.Format("{0, -4}", track.original_bit_rate);
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

        public void BeginTrack(wireless_android_skyjam.Track track)
        {
            if (this.lastArtist != track.artist)
            {
                this.lastArtist = track.artist;
                Console.Write("Artist: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(track.artist);
                Console.ResetColor();
            }
            if (this.lastAlbum != track.album)
            {
                this.lastAlbum = track.album;
                Console.Write("Album: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(track.album);
                Console.ResetColor();
            }

            this.track = track;
            this.SetProgress(string.Empty, false);
        }

        public void BeginMetadata(wireless_android_skyjam.Track track)
        {
            this.SetProgress("Sending metadata...", false);
        }

        public void MetadataMatch(wireless_android_skyjam.Track track)
        {
            this.SetProgress("Metadata matched...", false);
        }

        public void MetadataMatchRetry(wireless_android_skyjam.Track track, int matchRetryCount)
        {
            this.SetProgress("Metadata retry " + matchRetryCount, false);
        }

        public void MetadataNoMatch(wireless_android_skyjam.Track track)
        {
            this.SetProgress("Metadata no match", false);
        }

        public void BeginUploadSample(wireless_android_skyjam.Track track)
        {
            this.SetProgress("Uploading sample", true);
        }

        public void EndUploadSample(wireless_android_skyjam.Track track, wireless_android_skyjam.TrackSampleResponse.ResponseCode responseCode)
        {
            this.SetProgress("Sample result: " + responseCode.ToString().ToLower().Replace("_", " "), false);
        }


        public void BeginUploadTrack(wireless_android_skyjam.Track track)
        {
            this.SetProgress("Uploading track", true);
        }

        public void EndUploadTrack(wireless_android_skyjam.Track track, string status, string serverFileReference)
        {
            this.SetProgress("Upload track result: " + status.ToString().ToLower().Replace("_", " "), false);
        }

        public void EndTrack(Track track)
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


        public void BeginSessionRequest(Track track)
        {
            this.SetProgress("Uploading session request...", false);
        }

        public void RetrySessionRequest(Track track, int retryCount)
        {
            this.SetProgress("Retry session request " + retryCount, false);
        }

        public void EndSessionRequest(Track track)
        {
            this.SetProgress("Received session", false);
        }
    }
}
