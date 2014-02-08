using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerCLI
{
    internal class TrackUploadState
    {
        public string FileName { get; set; }
        public string ClientId
        {
            get
            {
                return this.Track.client_id;
            }
        }
        public Track Track { get; set; }
        public SignedChallengeInfo SignedChallengeInfo { get; set; }
        public TrackSampleResponse TrackSampleResponse { get; set; }
    }
}
