using GoogleMusicManagerAPI.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wireless_android_skyjam;
namespace GoogleMusicManagerAPI
{
    public interface IMusicManagerAPI
    {
        Task<UploadResponse> UploaderAuthenticate();
        Track BuildTrack(string filename);
        Task<UploadResponse> UploadMetadata(IEnumerable<Track> tracks);
        TrackSample BuildTrackSample(Track track, SignedChallengeInfo challenge, string filename);
        Task<UploadResponse> UploadSample(IEnumerable<TrackSample> tracks);
        Task<JsonUploadResponse> UploadTrack(Track track, TrackSampleResponse tsr, string fullFileName, int position, int trackCount);
    }
}
