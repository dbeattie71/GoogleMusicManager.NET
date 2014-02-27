using GoogleMusicManagerAPI.DeviceId;
using GoogleMusicManagerAPI.HTTPHeaders;
using GoogleMusicManagerAPI.Messages;
using GoogleMusicManagerAPI.TrackMetadata;
using GoogleMusicManagerAPI.TrackSampleEncoder;
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
    public class UploadProcess
    {
        private IMusicManagerAPI api;
        private Oauth2API oauthApi;
        private IUploadProcessObserver observer;

        public UploadProcess(IOauthTokenStorage tokenStorage, IUploadProcessObserver observer, ITrackSampleEncoder encoder)
        {
            var progressHandler = new ProgressMessageHandler();
            progressHandler.HttpSendProgress += progressHandler_HttpSendProgress;
            progressHandler.HttpReceiveProgress += progressHandler_HttpReceiveProgress;

            var client = new GoogleOauth2HTTP(
                new List<IHttpHeaderBuilder>()
                {
                    new MusicManagerHeaderBuilder(),
                    new Oauth2HeaderBuilder(tokenStorage),
                }, progressHandler
            );
            this.api = new MusicManagerAPI(
                client,
                new MacAddressDeviceId(),
                encoder
                );
            this.oauthApi = new Oauth2API(tokenStorage);
            this.observer = observer;
        }

        void progressHandler_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
        }

        void progressHandler_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            this.observer.SendProgress(e.ProgressPercentage);
        }

        public async Task<bool> DoUpload(IEnumerable<ITrackMetadata> fileList, bool uploadTracks)
        {
            await this.OauthAuthenticate();
            await this.AuthenticateUploader();

            var uploadState = this.BuildUploadState(fileList);

            await api.UpdateUploadStateStart();
            foreach (var us in uploadState.OrderBy(p => p.Track.artist).ThenBy(p => p.Track.year).ThenBy(p => p.Track.album).ThenBy(p => p.Track.disc_number).ThenBy(p => p.Track.track_number))
            {
                this.observer.BeginTrack(us.TrackMetaData);
                await this.UploadTrack(us, uploadState.IndexOf(us) + 1, uploadState.Count, uploadTracks);
                this.observer.EndTrack(us.TrackMetaData);
            }
            await api.UpdateUploadStateStopped();
            return true;
        }

        private async Task<bool> UploadTrack(TrackUploadState us, int position, int trackCount, bool uploadTrack)
        {
            var matchRetryCount = 0;

            this.observer.BeginMetadata(us.TrackMetaData);
            while (us.SignedChallengeInfo == null)
            {
                await this.UploadMetadata(new List<TrackUploadState>() { us });
                if (us.SignedChallengeInfo != null)
                {
                    this.observer.MetadataMatch(us.TrackMetaData);
                }
                else if (matchRetryCount < 10)
                {
                    matchRetryCount++;
                    var newClientId = GetRandomClientId(us.TrackMetaData.FileName, matchRetryCount);
                    us.Track.client_id = newClientId;
                    this.observer.MetadataMatchRetry(us.TrackMetaData, matchRetryCount);
                }
                else
                {
                    this.observer.MetadataNoMatch(us.TrackMetaData);
                    break;
                }
            }

            if (us.SignedChallengeInfo != null)
            {
                observer.BeginUploadSample(us.TrackMetaData);
                await this.UploadSample(new List<TrackUploadState>() { us });
                observer.EndUploadSample(us.TrackMetaData, us.TrackSampleResponse.response_code.ToString());
            }

            if (uploadTrack && us.TrackSampleResponse != null && us.TrackSampleResponse.response_code == TrackSampleResponse.ResponseCode.UPLOAD_REQUESTED)
            {
                var uploadSessionResponse = await GetUploadSession(us, position, trackCount);

                observer.BeginUploadTrack(us.TrackMetaData);
                var uploadResult = await api.UploadTrack(uploadSessionResponse, us.TrackMetaData.FileName);
                observer.EndUploadTrack(us.TrackMetaData,
                    uploadResult.sessionStatus.externalFieldTransfers.First().status,
                    uploadResult.sessionStatus.additionalInfo.googleRupioAdditionalInfo.completionInfo.customerSpecificInfo.ServerFileReference
                    );
            }

            return true;
        }

        private async Task<UploadSessionResponse> GetUploadSession(TrackUploadState us, int position, int trackCount)
        {
            var retryCount = 0;
            observer.BeginSessionRequest(us.TrackMetaData);
            while (true)
            {
                var uploadSessionResponse = await api.GetUploadSession(us.TrackMetaData.FileName, us.Track, us.TrackSampleResponse, position, trackCount);

                if (uploadSessionResponse.sessionStatus != null)
                {
                    observer.EndSessionRequest(us.TrackMetaData);
                    return uploadSessionResponse;
                }
                else
                {
                    await Task.Delay(1000);
                    retryCount++;
                    observer.RetrySessionRequest(us.TrackMetaData, retryCount);
                }
            }
        }

        private async Task<bool> UploadSample(IEnumerable<TrackUploadState> uploadStateList)
        {
            var trackSamples = uploadStateList.Select(p => api.BuildTrackSample(p.Track, p.SignedChallengeInfo, p.TrackMetaData.FileName)).ToList();

            var response = await api.UploadSample(trackSamples);

            uploadStateList.ToList().ForEach(p => p.TrackSampleResponse = response.sample_response.track_sample_response.Single(q => q.client_track_id == p.Track.client_id));
            return true;
        }



        private async Task<IEnumerable<TrackUploadState>> UploadMetadata(List<TrackUploadState> uploadState)
        {
            var trackList = uploadState.Select(p => p.Track).ToList();

            var response = await api.UploadMetadata(trackList);

            uploadState.ForEach(p => p.SignedChallengeInfo = response.metadata_response.signed_challenge_info.SingleOrDefault(q => q.challenge_info.client_track_id == p.ClientId));
            uploadState.ForEach(p => p.TrackSampleResponse = response.metadata_response.track_sample_response.SingleOrDefault(q => q.client_track_id == p.ClientId));
            return uploadState;
        }

        private List<TrackUploadState> BuildUploadState(IEnumerable<ITrackMetadata> fileList)
        {
            var uploadState = fileList.OrderBy(p => p.FileName).Select(p => new TrackUploadState()
            {
                TrackMetaData = p,
                Track = api.BuildTrack(p),
            }).ToList();

            return uploadState;
        }

        private static string GetRandomClientId(string filename, int adjustment)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = File.ReadAllBytes(filename);
            Array.Resize(ref inputBytes, inputBytes.Length + adjustment);
            var hash = md5.ComputeHash(inputBytes);
            var client_id = System.Convert.ToBase64String(hash).Replace("=", string.Empty);
            return client_id;
        }


        private async Task<bool> AuthenticateUploader()
        {
            var results = await api.UploaderAuthenticate();

            if (results.auth_status != UploadResponse.AuthStatus.OK)
            {
                throw new ApplicationException(results.auth_status.ToString());
            }

            return true;
        }

        private async Task<Oauth2Token> OauthAuthenticate()
        {
            var token = await oauthApi.Authenticate();
            return token;
        }

        //private T ReadFromFile<T>(string filename)
        //{
        //    var bytes = File.ReadAllBytes(filename);
        //    using (var ms = new MemoryStream(bytes))
        //    {
        //        var uploadResponse = Serializer.Deserialize<T>(ms);
        //        return uploadResponse;
        //    }
        //}

        //private void WriteToFile(string filename, object o)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        Serializer.Serialize(ms, o);
        //        ms.Position = 0;
        //        File.WriteAllBytes(filename, ms.ToArray());
        //    }
        //}


    }




}
