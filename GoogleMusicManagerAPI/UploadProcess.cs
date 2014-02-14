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

        public UploadProcess(IOauthTokenStorage tokenStorage, IUploadProcessObserver observer)
        {
            var progressHandler = new ProgressMessageHandler();
            progressHandler.HttpSendProgress += progressHandler_HttpSendProgress;
            progressHandler.HttpReceiveProgress += progressHandler_HttpReceiveProgress;

            var client = new GoogleOauth2HTTP(tokenStorage, progressHandler);
            this.api = new MusicManagerAPI(client);
            this.oauthApi = new Oauth2API(tokenStorage);
            this.observer = observer;
        }

        void progressHandler_HttpReceiveProgress(object sender, HttpProgressEventArgs e)
        {
            // Debug.WriteLine("Receive: " + e.ProgressPercentage.ToString());

            // Console.WriteLine(e.ProgressPercentage.ToString("   "));
        }

        void progressHandler_HttpSendProgress(object sender, HttpProgressEventArgs e)
        {
            // Debug.WriteLine("Send: " + e.ProgressPercentage.ToString());
            //var progress = e.ProgressPercentage.ToString();
            //progress = "\b\b\b" + new String(' ', 3 - progress.Length) + progress;

            this.observer.SendProgress(e.ProgressPercentage);

            //Console.Write(progress);
        }


        public async Task<bool> DoUpload(IEnumerable<string> fileList)
        {
            await this.OauthAuthenticate();
            await this.AuthenticateUploader();

            var uploadState = this.BuildUploadState(fileList);

            foreach (var us in uploadState.OrderBy(p => p.Track.artist).ThenBy(p => p.Track.year).ThenBy(p => p.Track.album).ThenBy(p => p.Track.disc_number).ThenBy(p=> p.Track.track_number))
            {
                this.observer.BeginTrack(us.Track);
                await this.UploadTrack(us, uploadState.IndexOf(us) + 1, uploadState.Count);
                this.observer.EndTrack(us.Track);
            }
            
            return true;
        }

        private async Task<bool> UploadTrack(TrackUploadState us, int position, int trackCount)
        {
            var matchRetryCount = 0;

            this.observer.BeginMetadata(us.Track);
            while (us.SignedChallengeInfo == null)
            {
                await this.UploadMetadata(new List<TrackUploadState>() { us });
                if (us.SignedChallengeInfo != null)
                {
                    this.observer.MetadataMatch(us.Track);
                }
                else if(matchRetryCount < 10)
                {
                    matchRetryCount++;
                    var newClientId = GetRandomClientId(us.FileName, matchRetryCount);
                    us.Track.client_id = newClientId;
                    this.observer.MetadataMatchRetry(us.Track, matchRetryCount);
                }
                else
                {
                    this.observer.MetadataNoMatch(us.Track);
                    break;
                }
            }

            if (us.SignedChallengeInfo != null)
            {
                observer.BeginUploadSample(us.Track);
                await this.UploadSample(new List<TrackUploadState>() { us });
                observer.EndUploadSample(us.Track, us.TrackSampleResponse.response_code);
            }

            if (us.TrackSampleResponse != null && us.TrackSampleResponse.response_code == TrackSampleResponse.ResponseCode.UPLOAD_REQUESTED)
            {
                observer.BeginUploadTrack(us.Track);
                var uploadResult = await api.UploadTrack(us.Track, us.TrackSampleResponse, us.FileName, position, trackCount);
                observer.EndUploadTrack(us.Track, 
                    uploadResult.sessionStatus.externalFieldTransfers.First().status,
                    uploadResult.sessionStatus.additionalInfo.googleRupioAdditionalInfo.completionInfo.customerSpecificInfo.ServerFileReference
                    );
            }

            return true;
        }


        private async Task<bool> UploadSample(IEnumerable<TrackUploadState> uploadStateList)
        {
            var trackSamples = uploadStateList.Select(p => api.BuildTrackSample(p.Track, p.SignedChallengeInfo, p.FileName)).ToList();

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

        private List<TrackUploadState> BuildUploadState(IEnumerable<string> fileList)
        {
            var uploadState = fileList.OrderBy(p => p).Select(p => new TrackUploadState()
            {
                FileName = p,
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
