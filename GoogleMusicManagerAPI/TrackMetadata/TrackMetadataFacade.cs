using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackMetadata
{
    public class TrackMetadataFacade
    {
        public ITrackMetadata CreateTrackMetadata(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var lastwrite = File.GetLastWriteTimeUtc(filename);
            var tagLibFile = TagLib.File.Create(filename);
            var tags = tagLibFile.Tag;
            var properties = tagLibFile.Properties;

            var track = new TrackMetadata()
            {
                Album = tags.Album,
                AlbumArtist = tags.JoinedAlbumArtists,
                Artist = tags.JoinedArtists,
                Title = tags.Title,
                Year = tags.Year,
                Genre = tags.JoinedGenres,
                DiscNumber = tags.Disc,
                TotalDiscCount = tags.DiscCount,
                TotalTrackCount = tags.TrackCount,
                Duration = properties.Duration,
                AudioBitrate = properties.AudioBitrate,
                TrackNumber = tags.Track,
                FileSize = fileInfo.Length,
                LastModified = lastwrite,
                FileName = filename,
            };

            if (track.Title != null) track.Title = track.Title.Trim();
            if (track.Album != null) track.Album = track.Album.Trim();
            if (track.Artist != null) track.Artist = track.Artist.Trim();
            if (string.IsNullOrWhiteSpace(track.AlbumArtist)) track.AlbumArtist = track.Artist;
            return track;



        }
    }
}
