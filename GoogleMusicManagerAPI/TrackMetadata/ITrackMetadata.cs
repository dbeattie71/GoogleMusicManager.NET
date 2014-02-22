using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackMetadata
{
    public interface ITrackMetadata
    {
        string Album { get; }
        string AlbumArtist { get; }
        string Artist { get; }
        string Title { get; }
        uint Year { get; }
        string Genre { get; }
        uint DiscNumber { get; }
        uint TotalDiscCount { get; }
        uint TotalTrackCount { get; }
        TimeSpan Duration { get; }
        int AudioBitrate { get; }
        uint TrackNumber { get; }
        long FileSize { get; }
        string FileName { get; }
        DateTime LastModified { get; }

    }
}
