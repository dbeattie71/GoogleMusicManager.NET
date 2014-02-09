﻿using GoogleMusicAPICLI;
using GoogleMusicManagerAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wireless_android_skyjam;

namespace GoogleMusicManagerCLI
{
    class UploadProcess
    {
        private IMusicManagerAPI api;
        private Oauth2API oauthApi;

        public UploadProcess()
        {
            var tokenStorage = new OauthTokenStorage();
            this.api = new MusicManagerAPI(tokenStorage);
            this.oauthApi = new Oauth2API(tokenStorage);
        }
        
        public async Task<bool> DoUpload(string[] fileList)
        {
            await this.OauthAuthenticate();
            await this.AuthenticateUploader();

            var uploadState = this.BuildUploadState(fileList);
            foreach (var artist in uploadState.Select(p => p.Track).Select(p => p.artist).Distinct().OrderBy(p => p))
            {
                Console.WriteLine("Artist: " + artist);

                foreach (var album in uploadState.Select(p => p.Track).Where(p => p.artist == artist).Select(p => p.album).Distinct().OrderBy(p => p))
                {
                    Console.WriteLine("Album: " + album);

                    foreach (var us in uploadState.Where(p => p.Track.artist == artist && p.Track.album == album).OrderBy(p => p.Track.disc_number).OrderBy(p => p.Track.track_number))
                    {
                        await this.UploadTrack(us, uploadState.IndexOf(us) + 1, uploadState.Count);
                    }
                }
            }
            return true;
        }

        private async Task<bool> UploadTrack(TrackUploadState us, int position, int trackCount)
        {
            var matchSuccess = false;
            var matchRetryCount = 0;

            Console.WriteLine("Track:" + us.Track.track_number + "\t" + us.Track.title);

            while (matchSuccess == false && matchRetryCount < 5)
            {
                await this.UploadMetadata(new List<TrackUploadState>() { us });
                if (us.SignedChallengeInfo != null)
                {
                    matchSuccess = true;
                    Console.WriteLine("\tMetadata accepted");
                }
                else
                {
                    matchRetryCount++;
                    var newClientId = GetRandomClientId(us.FileName, matchRetryCount);
                    us.Track.client_id = newClientId;
                    Console.WriteLine("\tMetadata retry " + matchRetryCount);
                }
            }

            if (us.SignedChallengeInfo != null)
            {
                await this.UploadSample(new List<TrackUploadState>() { us });
                Console.WriteLine("\tSample result: " + us.TrackSampleResponse.response_code);
            }

            if (us.TrackSampleResponse != null && us.TrackSampleResponse.response_code == TrackSampleResponse.ResponseCode.UPLOAD_REQUESTED)
            {
                var uploadResult = await api.UploadTrack(us.Track, us.TrackSampleResponse, us.FileName, position, trackCount);
                Console.Write("\tUpload: " + uploadResult.sessionStatus.externalFieldTransfers.First().status);
                Console.WriteLine(", serverId = " + uploadResult.sessionStatus.additionalInfo.googleRupioAdditionalInfo.completionInfo.customerSpecificInfo.ServerFileReference);
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
