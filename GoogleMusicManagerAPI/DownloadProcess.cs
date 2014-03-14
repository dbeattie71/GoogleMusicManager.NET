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

        public async Task<bool> DoDownload(string artist, string album, string title)
        {
            var oauthApi = new Oauth2API(oauth2Storage);

            await oauthApi.Authenticate();
            await api.UploaderAuthenticate();

            var trackList = await this.GetTracks();

            var tracksToDownload = trackList.Where(p => string.IsNullOrEmpty(artist) || p.artist.ToLower().Contains(artist.ToLower()))
                .Where(p => string.IsNullOrEmpty(album) || p.album.ToLower().Contains(album.ToLower()))
                .Where(p => string.IsNullOrEmpty(title) || p.title.ToLower().Contains(title.ToLower()))
                .OrderBy(p => string.IsNullOrEmpty(p.album_artist) ? p.artist : p.album_artist)
                .ThenBy(p => p.album)
                .ThenBy(p => p.track_number)
                .ThenBy(p => p.title)
                .ToList();

            this.observer.BeginDownloadTracks(tracksToDownload.Select(p => CreateTrackMetadata(p)));

            foreach (var track in tracksToDownload)
            {
                this.observer.BeginDownloadTrack(CreateTrackMetadata(track));

                var filename = GetOutputFilename(track);
                var folderPath = GetOutputDirectory(track);
                var fullpath = Path.Combine(folderPath, filename);

                if (ShouldDownload(fullpath, track.track_size))
                {
                    var songid = track.id;
                    var info = await api.GetTracksUrl(songid);
                    var bytes = await api.DownloadTrack(info.url);
                    Directory.CreateDirectory(folderPath);
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

        private static string GetOutputDirectory(DownloadTrackInfo track)
        {
            var firstDirectoryLevel = string.IsNullOrEmpty(track.album_artist) ? track.artist : track.album_artist;
            Path.GetInvalidPathChars().ToList().ForEach(p => firstDirectoryLevel = firstDirectoryLevel.Replace(p.ToString(), ""));
            var secondDirectoryLevel = track.album;
            Path.GetInvalidPathChars().ToList().ForEach(p => secondDirectoryLevel = secondDirectoryLevel.Replace(p.ToString(), ""));
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
