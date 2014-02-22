using GoogleMusicManagerAPI.TrackMetadata;
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
        void BeginTrack(ITrackMetadata track);
        void BeginMetadata(ITrackMetadata track);
        void MetadataMatch(ITrackMetadata track);
        void MetadataMatchRetry(ITrackMetadata track, int matchRetryCount);
        void MetadataNoMatch(ITrackMetadata track);
        void BeginUploadSample(ITrackMetadata track);
        void EndUploadSample(ITrackMetadata track, string responseCode);
        void BeginSessionRequest(ITrackMetadata track);
        void RetrySessionRequest(ITrackMetadata track, int retryCount);
        void EndSessionRequest(ITrackMetadata track);
        void BeginUploadTrack(ITrackMetadata track);
        void EndUploadTrack(ITrackMetadata track, string status, string serverFileReference);
        void EndTrack(ITrackMetadata track);

        void SendProgress(int p);
    }
}
