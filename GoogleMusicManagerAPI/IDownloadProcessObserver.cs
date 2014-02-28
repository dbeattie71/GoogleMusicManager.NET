using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI
{
    public interface IDownloadProcessObserver
    {
        void BeginDownloadTracks(IEnumerable<ITrackMetadata> trackMetadata);

        void DownloadTrackExists(ITrackMetadata trackMetadata);

        void BeginDownloadTrack(ITrackMetadata trackMetadata);

        void EndDownloadTrack(ITrackMetadata trackMetadata);
    }
}
