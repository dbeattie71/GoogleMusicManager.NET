using GoogleMusicManagerAPI.Messages;
using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wireless_android_skyjam;
namespace GoogleMusicManagerAPI
{
    public interface IMusicManagerAPI
    {
        Task<UploadResponse> UploaderAuthenticate();
        Track BuildTrack(ITrackMetadata filename);
        Task<UploadResponse> UploadMetadata(IEnumerable<Track> tracks);
        TrackSample BuildTrackSample(Track track, SignedChallengeInfo challenge, string filename);
        Task<UploadResponse> UploadSample(IEnumerable<TrackSample> tracks);
        Task<bool> UpdateUploadStateStart();
        Task<bool> UpdateUploadStateStopped();
        Task<bool> UpdateUploadStatePaused();
        Task<UploadSessionResponse> GetUploadSession(string fullFileName, Track track, TrackSampleResponse tsr, int position, int trackCount);
        Task<JsonUploadResponse> UploadTrack(UploadSessionResponse uploadSessionResponse, string fullFileName);
        Task<GetTracksToExportResponse> GetTracksToExport(string continuationToken);
        Task<ExportUrl> GetTracksUrl(string songId);
        Task<byte[]> DownloadTrack(string url);
    }
}
