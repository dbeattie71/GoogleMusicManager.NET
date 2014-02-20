using GoogleMusicManagerAPI.Messages;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerAPI
{
    public class MusicManagerAPI : GoogleMusicManagerAPI.IMusicManagerAPI
    {
        IGoogleOauth2HTTP oauth2Client;
        IDeviceId clientId;

        public MusicManagerAPI(IGoogleOauth2HTTP oauth2Client, IDeviceId clientId)
        {
            this.oauth2Client = oauth2Client;
            this.clientId = clientId;
        }

        public async Task<UploadResponse> UploaderAuthenticate()
        {
            var upauthRequest = this.CreateUpauthRequest();
            using (var ms = this.SerializeMessage(upauthRequest))
            {
                var results = await oauth2Client.Request(HttpMethod.Post, new Uri("https://android.clients.google.com/upsj/upauth"), ms);
                var uploaderResponse = Serializer.Deserialize<UploadResponse>(results);
                return uploaderResponse;
            }
        }

        public async Task<UploadResponse> UploadMetadata(IEnumerable<Track> tracks)
        {
            var request = this.CreateUploadMetadataRequest(tracks);
            using (var ms = this.SerializeMessage(request))
            {
                using (var results = await oauth2Client.Request(HttpMethod.Post, new Uri("https://android.clients.google.com/upsj/metadata?version=1"), ms))
                {
                    var uploaderResponse = Serializer.Deserialize<UploadResponse>(results);
                    return uploaderResponse;
                }
            }
        }

        public UploadMetadataRequest CreateUploadMetadataRequest(IEnumerable<Track> tracks)
        {
            var uploadMetaDataRequest = new UploadMetadataRequest()
            {
                uploader_id = clientId.GetDeviceId(),
            };

            uploadMetaDataRequest.track.AddRange(tracks);

            return uploadMetaDataRequest;
        }

        public async Task<UploadResponse> UploadSample(IEnumerable<TrackSample> tracks)
        {
            var request = this.CreateUploadSampleRequest(tracks);
            var requestStream = this.SerializeMessage(request);
            using (var results = await oauth2Client.Request(HttpMethod.Post, new Uri("https://android.clients.google.com/upsj/sample?version=1"), requestStream))
            {
                var uploaderResponse = Serializer.Deserialize<UploadResponse>(results);
                return uploaderResponse;
            }
        }

        private UploadSampleRequest CreateUploadSampleRequest(IEnumerable<TrackSample> trackSample)
        {
            var uploadSampleRequest = new UploadSampleRequest()
            {
                uploader_id = clientId.GetDeviceId(),
            };

            uploadSampleRequest.track_sample.AddRange(trackSample);

            return uploadSampleRequest;
        }

        private MemoryStream SerializeMessage(object message)
        {
            var ms = new MemoryStream();
            Serializer.Serialize(ms, message);
            ms.Position = 0;
            return ms;
        }

        private UpAuthRequest CreateUpauthRequest()
        {
            var test = new UpAuthRequest();
            test.uploader_id = clientId.GetDeviceId();
            test.friendly_name = this.GetUploaderName();
            return test;
        }

        private string GetUploaderName()
        {
            return this.GetMachineName() + " (GoogleMusicManager.NET 0.1)";
        }

        private string GetMachineName()
        {
            return System.Net.Dns.GetHostName();
        }

        public async Task<JsonUploadResponse> UploadTrack(UploadSessionResponse uploadSessionResponse, string fullFileName)
        {
            var uploadUrl = uploadSessionResponse.sessionStatus.externalFieldTransfers[0].putInfo.url;
            using (var fileBytes = new FileStream(fullFileName, FileMode.Open, FileAccess.Read))
            {
                using (var uploadResults = await oauth2Client.Request(HttpMethod.Put, new Uri(uploadUrl), fileBytes))
                {
                    var uploadResponse = DeserializeJsonStream<JsonUploadResponse>(uploadResults);
                    return uploadResponse;
                }
            }
        }

        private static T DeserializeJsonStream<T>(Stream uploadResults)
        {
            var jsonSerializer = new JsonSerializer();
            var uploadResponse = jsonSerializer.Deserialize<T>(new JsonTextReader(new StreamReader(uploadResults)));
            return uploadResponse;
        }

        public async Task<UploadSessionResponse> GetUploadSession(string fullFileName, Track track, TrackSampleResponse tsr, int position, int trackCount)
        {
            var uploadSessionRequest = BuildUploadSessionRequest(fullFileName, track, tsr, position, trackCount);

            var uploadSessionRequestString = JsonConvert.SerializeObject(uploadSessionRequest);
            var uploadSessionRequestBytes = System.Text.Encoding.UTF8.GetBytes(uploadSessionRequestString);

            using (var ms = new MemoryStream(uploadSessionRequestBytes))
            {
                var results = await oauth2Client.Request(HttpMethod.Post, new Uri("https://uploadsj.clients.google.com/uploadsj/rupio"), ms);
                var uploadSessionResponse = DeserializeJsonStream<UploadSessionResponse>(results);
                return uploadSessionResponse;

            }
        }

        private UploadSessionRequest BuildUploadSessionRequest(string fullFileName, Track track, TrackSampleResponse tsr, int position, int trackCount)
        {
            var fullPath = fullFileName;
            var fileName = Path.GetFileName(fullPath);

            var uploadSessionRequest = new UploadSessionRequest()
            {
                clientId = "Jumper Uploader",
                protocolVersion = "0.8",
                createSessionRequest = new UploadSessionRequest.CreateSessionRequest()
                {
                    fields = new List<UploadSessionRequest.CreateSessionRequest.Field>()
                        {
                            new UploadSessionRequest.CreateSessionRequest.External(fullPath, fileName),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("SyncNow", "true"),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("ClientTotalSongCount", trackCount.ToString()),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("CurrentUploadingTrack", track.title),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("CurrentTotalUploadedCount", position.ToString()),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("title", "jumper-uploader-title-42"),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("TrackDoNotRematch", "false"),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("ServerId", tsr.server_track_id),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("UploaderId", clientId.GetDeviceId()),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("TrackBitRate", track.original_bit_rate.ToString()),
                            new UploadSessionRequest.CreateSessionRequest.Inlined("ClientId", track.client_id),
                        },
                },
            };
            return uploadSessionRequest;
        }

        public Track BuildTrack(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var lastwrite = File.GetLastWriteTimeUtc(filename);
            var tagLibFile = TagLib.File.Create(filename);
            var tags = tagLibFile.Tag;
            var properties = tagLibFile.Properties;

            var track = new Track()
            {
                album = tags.Album,
                album_artist = tags.JoinedAlbumArtists,
                artist = tags.JoinedArtists,
                title = tags.Title,
                year = (int)tags.Year,
                genre = tags.JoinedGenres,
                disc_number = (int)tags.Disc,
                total_disc_count = (int)tags.DiscCount,
                total_track_count = (int)tags.TrackCount,
                duration_millis = (long)properties.Duration.TotalMilliseconds, //    (properties.Duration.TotalSeconds -1) * 1000,
                original_bit_rate = properties.AudioBitrate,
                track_number = (int)tags.Track,
                client_id = GetClientIdForFile(filename),
                estimated_size = fileInfo.Length,
                last_modified_timestamp = this.ConvertToTimestamp(lastwrite),
            };

            return track;
        }

        private string GetClientIdForFile(string filename)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = File.ReadAllBytes(filename);
            var hash = md5.ComputeHash(inputBytes);
            var client_id = System.Convert.ToBase64String(hash).Replace("=", string.Empty);
            return client_id;
        }

        private long ConvertToTimestamp(DateTime value)
        {
            var span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }

        public TrackSample BuildTrackSample(Track track, SignedChallengeInfo challenge, string filename)
        {
            var ts = new TrackSample();
            ts.signed_challenge_info = challenge;
            ts.track = track;

            var start = challenge.challenge_info.start_millis / 1000;
            var duration = challenge.challenge_info.duration_millis / 1000;

            var bytes = GetMP3Sample(filename, (int)start, (int)duration);
            ts.sample = bytes;
            //ts.sample = new byte[100];
            return ts;
        }

        private static byte[] GetMP3Sample(string filename, int start, int duration)
        {
            var tempFileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(filename) + "_sample" + ".mp3";
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            var argumentTemplate = "-i \"{0}\" -t {1} -ss {2} -ab 128k -f s16le -c libmp3lame \"{3}\"";

            var populatedArguments = string.Format(argumentTemplate, filename, duration, start, tempFileName);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "avconv.exe",
                    Arguments = populatedArguments,
                    WindowStyle = ProcessWindowStyle.Minimized,
                }
            };
            process.Start();
            process.WaitForExit();

            var bytes = File.ReadAllBytes(tempFileName);
            File.Delete(tempFileName);
            return bytes;
        }

        public async Task<GetTracksToExportResponse> GetTracksToExport(string continuationToken)
        {
            var request = new GetTracksToExportRequest();
            request.client_id = clientId.GetDeviceId();
            request.export_type = GetTracksToExportRequest.TracksToExportType.ALL;
            request.continuation_token = continuationToken;

            var requestStream = this.SerializeMessage(request);
            using (var results = await oauth2Client.Request(HttpMethod.Post, new Uri("https://music.google.com/music/exportids"), requestStream))
            {
                var uploaderResponse = Serializer.Deserialize<GetTracksToExportResponse>(results);
                return uploaderResponse;
            }
        }

        public async Task<ExportUrl> GetTracksUrl(string songId)
        {
            var url = "https://music.google.com/music/export?version=2&songid=" + songId;

            using (var results = await oauth2Client.Request(HttpMethod.Get, new Uri(url)))
            {
                var exportUrl = DeserializeJsonStream<ExportUrl>(results);
                return exportUrl;
            }
        }

        public async Task<byte[]> DownloadTrack(string url)
        {
            using (var results = await oauth2Client.Request(HttpMethod.Get, new Uri(url)))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    results.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

    }
}
