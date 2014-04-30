using GoogleMusicManagerAPI.DeviceId;
using GoogleMusicManagerAPI.HTTPHeaders;
using GoogleMusicManagerAPI.TrackMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerAPI
{
    public class DownloadProcess
    {
        private IOauthTokenStorage oauth2Storage { get; set; }
        private IMusicManagerAPI api { get; set; }
        private IDownloadProcessObserver observer { get; set; }

        public DownloadProcess(
            IOauthTokenStorage oauth2Storage,
            IDownloadProcessObserver observer
            )
        {
            var progressHandler = new ProgressMessageHandler();
            progressHandler.HttpSendProgress += progressHandler_HttpSendProgress;
            progressHandler.HttpReceiveProgress += progressHandler_HttpReceiveProgress;

            var deviceId = new MacAddressDeviceId();
            this.oauth2Storage = oauth2Storage;
            this.api = new MusicManagerAPI(
                new GoogleOauth2HTTP(
                    new List<IHttpHeaderBuilder> {
                                    new Oauth2HeaderBuilder(oauth2Storage),
                                    new MusicManagerHeaderBuilder(),
                                    new DeviceIDHeaderBuilder(deviceId),
                                },
                    progressHandler),
                    deviceId
                );
            this.observer = observer;
        }

        void progressHandler_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
            this.observer.ReceiveProgress(e.ProgressPercentage);
        }

        void progressHandler_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
        }

        private class TrackPath
        {
            public DownloadTrackInfo Track { get; set; }
            public string Filename { get; set; }
            public string Directory { get; set; }
        }


        public async Task<bool> DoDownload(string path, string artist, string album, string title)
        {
            var oauthApi = new Oauth2API(oauth2Storage);

            await oauthApi.Authenticate();
            await api.UploaderAuthenticate();

            var trackList = await this.GetTracks();

            var tracklistWithNames = trackList.Select(p =>
                new TrackPath
                {
                    Track = p,
                    Filename = GetOutputFilename(p),
                    Directory = GetOutputDirectory(p),
                }
            ).ToList();

            var duplicateFilenames = tracklistWithNames.GroupBy(x => x.Directory + x.Filename)
              .Where(g => g.Count() > 1)
              .SelectMany(p => p)
              .ToList();

            foreach (var duplicate in duplicateFilenames)
            {
                duplicate.Filename = GetOutputFilenameUnique(duplicate.Track);
            }

            var tracksToDownload = tracklistWithNames.Where(p => string.IsNullOrEmpty(artist) || p.Track.artist.ToLower().Contains(artist.ToLower()))
                 .Where(p => string.IsNullOrEmpty(album) || p.Track.album.ToLower().Contains(album.ToLower()))
                 .Where(p => string.IsNullOrEmpty(title) || p.Track.title.ToLower().Contains(title.ToLower()))
                 .OrderBy(p => string.IsNullOrEmpty(p.Track.album_artist) ? p.Track.artist : p.Track.album_artist)
                 .ThenBy(p => p.Track.album)
                 .ThenBy(p => p.Track.track_number)
                 .ThenBy(p => p.Track.title)
                 .ToList();

            this.observer.BeginDownloadTracks(tracksToDownload.Select(p => CreateTrackMetadata(p.Track)));

            foreach (var trackWithName in tracksToDownload)
            {
                var track = trackWithName.Track;
                this.observer.BeginDownloadTrack(CreateTrackMetadata(track));

                var filename = trackWithName.Filename;
                var folderPath = trackWithName.Directory;
                folderPath = Path.Combine(path, folderPath);
                var fullpath = Path.Combine(folderPath, filename);

                if (ShouldDownload(fullpath, track.track_size))
                {
                    var songid = track.id;
                    var info = await api.GetTracksUrl(songid);
                    var bytes = await api.DownloadTrack(info.url);
                    try
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    catch
                    {
                        throw new ApplicationException("Unable to create path: " + folderPath);
                    }

                    File.WriteAllBytes(fullpath, bytes);
                    this.observer.EndDownloadTrack(CreateTrackMetadata(track));
                }
                else
                {
                    this.observer.DownloadTrackExists(CreateTrackMetadata(track));
                }

            }

            return true;
        }

        private ITrackMetadata CreateTrackMetadata(DownloadTrackInfo p)
        {
            var trackMetadata = new TrackMetadata.TrackMetadata()
            {
                Album = p.album,
                AlbumArtist = p.album_artist,
                Artist = p.artist,
                DiscNumber = (uint)p.disc_number,
                FileSize = p.track_size,
                TotalDiscCount = (uint)p.total_disc_count,
                Title = p.title,
                TrackNumber = (uint)p.track_number,
            };
            return trackMetadata;
        }

        private static bool ShouldDownload(string filename, long size)
        {
            if (File.Exists(filename) == false) return true;

            return false;
            var fileInfo = new FileInfo(filename);

            return fileInfo.Length != size;
        }

        private static string GetOutputFilename(DownloadTrackInfo track)
        {
            var filename = track.track_number + " " + track.title + ".mp3";
            Path.GetInvalidFileNameChars().ToList().ForEach(p => filename = filename.Replace(p.ToString(), ""));
            return filename;
        }

        private static string GetOutputFilenameUnique(DownloadTrackInfo track)
        {
            var filename = track.track_number + " " + track.title + " " + track.id + ".mp3";
            Path.GetInvalidFileNameChars().ToList().ForEach(p => filename = filename.Replace(p.ToString(), ""));
            return filename;
        }

        private static string GetOutputDirectory(DownloadTrackInfo track)
        {
            var invalidPathChars = Path.GetInvalidPathChars().Select(p => p.ToString()).ToList();
            invalidPathChars.Add(":");
            invalidPathChars.Add("?");

            var firstDirectoryLevel = string.IsNullOrEmpty(track.album_artist) ? track.artist : track.album_artist;
            invalidPathChars.ForEach(p => firstDirectoryLevel = firstDirectoryLevel.Replace(p, ""));
            var secondDirectoryLevel = track.album;
            invalidPathChars.ForEach(p => secondDirectoryLevel = secondDirectoryLevel.Replace(p, ""));
            var folderPath = Path.Combine(firstDirectoryLevel, secondDirectoryLevel);
            return folderPath;
        }

        public async Task<IEnumerable<DownloadTrackInfo>> GetTracks()
        {
            var trackList = new List<DownloadTrackInfo>();

            string token = null;

            var tracks = await api.GetTracksToExport(token);
            ValidateTrackExportResponse(tracks);
            trackList.AddRange(tracks.download_track_info);

            while (string.IsNullOrEmpty(tracks.continuation_token) == false)
            {
                tracks = await api.GetTracksToExport(tracks.continuation_token);
                ValidateTrackExportResponse(tracks);
                trackList.AddRange(tracks.download_track_info);
            }

            return trackList;
        }

        private static void ValidateTrackExportResponse(GetTracksToExportResponse tracks)
        {
            if (tracks.status != GetTracksToExportResponse.TracksToExportStatus.OK)
            {
                throw new ApplicationException("Received TracksToExportStatus: " + tracks.status);
            }
        }

    }
}
