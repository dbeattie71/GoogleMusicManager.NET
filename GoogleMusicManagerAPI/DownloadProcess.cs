using GoogleMusicManagerAPI.DeviceId;
using GoogleMusicManagerAPI.HTTPHeaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerAPI
{
    public class DownloadProcess
    {
        private IOauthTokenStorage oauth2Storage { get; set; }
        private IMusicManagerAPI api { get; set; }

        public DownloadProcess(IOauthTokenStorage oauth2Storage)
        {
            var deviceId = new MacAddressDeviceId();
            this.oauth2Storage = oauth2Storage;
            this.api = new MusicManagerAPI(
                new GoogleOauth2HTTP(
                    new List<IHttpHeaderBuilder> {
                                    new Oauth2HeaderBuilder(oauth2Storage),
                                    new MusicManagerHeaderBuilder(),
                                    new DeviceIDHeaderBuilder(deviceId),
                                }),
                    deviceId
                );
        }


        public async Task<bool> DoDownload(string artist, string album, string title)
        {
            var oauthApi = new Oauth2API(oauth2Storage);

            await oauthApi.Authenticate();
            await api.UploaderAuthenticate();

            var trackList = await this.GetTracks();

            var tracksToDownload = trackList.Where(p => string.IsNullOrEmpty(artist) || p.artist.ToLower().Contains(artist.ToLower()))
                .Where(p => string.IsNullOrEmpty(album) || p.album.ToLower().Contains(album.ToLower()))
                .Where(p => string.IsNullOrEmpty(title) || p.title.ToLower().Contains(title.ToLower()));

            foreach (var track in tracksToDownload)
            {
                var songid = track.id;

                var info = await api.GetTracksUrl(songid);

                var bytes = await api.DownloadTrack(info.url);

                var folderPath = GetOutputDirectory(track);
                Directory.CreateDirectory(folderPath);

                var filename = GetOutputFilename(track);
                
                var fullpath = Path.Combine(folderPath, filename);

                File.WriteAllBytes(fullpath, bytes);
            }

            return true;
        }

        private static string GetOutputFilename(DownloadTrackInfo track)
        {
            var filename = track.track_number + " " + track.title + ".mp3";
            Path.GetInvalidFileNameChars().ToList().ForEach(p => filename = filename.Replace(p.ToString(), ""));
            return filename;
        }

        private static string GetOutputDirectory(DownloadTrackInfo track)
        {
            var firstDirectoryLevel = track.artist;
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
