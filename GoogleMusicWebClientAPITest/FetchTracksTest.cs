using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class FetchTracksTest
    {
        [TestMethod]
        public void Test_FetchTracks()
        {
            var api = GoogleMusicWebClientInstance.GetInstance();

            var tracksTask = api.GetAllSongs();
            tracksTask.Wait();
            var tracks = tracksTask.Result;

            if (tracks.Any() == false)
            {
                Assert.Fail("No tracks to fetch");
            }

            var tracksToFetch = tracks.Where(p => string.IsNullOrEmpty(p.MatchedID) == false).Take(10);

            var fetchTracksTask = api.FetchTracks(tracksToFetch);
            fetchTracksTask.Wait();
            var fetchedTracks = fetchTracksTask.Result;

            Assert.AreEqual(tracksToFetch.Count(), fetchedTracks.Count());

        }
    }
}
