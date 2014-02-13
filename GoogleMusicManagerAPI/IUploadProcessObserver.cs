using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerAPI
{
    public interface IUploadProcessObserver
    {
        void BeginTrack(Track track);
        void BeginMetadata(Track track);
        void MetadataMatch(Track track);
        void MetadataMatchRetry(Track track, int matchRetryCount);
        void MetadataNoMatch(Track track);
        void BeginUploadSample(Track track);
        void EndUploadSample(Track track, TrackSampleResponse.ResponseCode responseCode);
        void BeginUploadTrack(Track track);
        void EndUploadTrack(Track track, string status, string serverFileReference);
        void EndTrack(Track track);

        void SendProgress(int p);
    }
}
