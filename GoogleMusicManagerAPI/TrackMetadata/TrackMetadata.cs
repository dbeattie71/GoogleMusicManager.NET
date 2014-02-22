using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackMetadata
{
    class TrackMetadata : ITrackMetadata
    {
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public uint Year { get; set; }
        public string Genre { get; set; }
        public uint DiscNumber { get; set; }
        public uint TotalDiscCount { get; set; }
        public uint TotalTrackCount { get; set; }
        public TimeSpan Duration { get; set; }
        public int AudioBitrate { get; set; }
        public uint TrackNumber { get; set; }
        public long FileSize { get; set; }
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
    }
}
