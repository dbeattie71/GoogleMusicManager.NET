using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using GoogleMusicWebClientAPI.Models;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class ModifyTrackTest
    {
        [TestMethod]
        public void Test_ModifyTrack()
        {
            var api = GoogleMusicWebClientInstance.GetInstance();

            //var tracks0 = GetAllTracks(api);

            //var tracksToModify0 = tracks0.Where(p => p.Title.EndsWith("*"));

            //foreach (var track in tracksToModify0)
            //{
            //    var diff = new GoogleMusicSongDiff();
            //    diff.ID = track.ID;
            //    diff.Title = track.Title.Replace("*","");

            //    ModifyTrack(api, diff);
            //}

            //return;

            var tracksToModify = GetSampleTrackSelection(api);

            foreach (var track in tracksToModify)
            {
                var diff = new GoogleMusicSongDiff();
                diff.ID = track.ID;
                diff.Title = track.Title + "_TEST";

                ModifyTrack(api, diff);  
            }

            var modifiedTracks = GetSampleTrackSelection(api);

            foreach (var track in modifiedTracks)
            {
                Assert.IsTrue(track.Title.EndsWith("_TEST"));

                var diff = new GoogleMusicSongDiff();
                diff.ID = track.ID;
                diff.Title = track.Title.Replace("_TEST","");

                ModifyTrack(api, diff);  
            }

            var revertedTracks = GetSampleTrackSelection(api);

            foreach (var track in revertedTracks)
            {
                Assert.IsFalse(track.Title.EndsWith("_TEST"));
            }

        }

        private static System.Collections.Generic.IEnumerable<GoogleMusicSong> GetSampleTrackSelection(GoogleMusicWebClientAPI.IGoogleMusicWebClient api)
        {
            var tracks = GetAllTracks(api);

            if (tracks.Any() == false)
            {
                Assert.Fail("No tracks to modify");
            }

            var tracksToModify = tracks.OrderBy(p => p.ID).Take(10);
            return tracksToModify;
        }

        private static void ModifyTrack(GoogleMusicWebClientAPI.IGoogleMusicWebClient api, GoogleMusicSongDiff diff)
        {
            var modifyTask = api.ModifyTrack(diff);
            modifyTask.Wait();
            var modifyResult = modifyTask.Result;
        }

        private static System.Collections.Generic.IEnumerable<GoogleMusicSong> GetAllTracks(GoogleMusicWebClientAPI.IGoogleMusicWebClient api)
        {
            var tracksTask = api.GetAllSongs();
            tracksTask.Wait();
            var tracks = tracksTask.Result;
            return tracks;
        }
    }
}
