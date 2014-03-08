using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class StreamingLoadAllTracksTest
    {
        [TestMethod]
        public void Test_StreamingLoadAllTracks()
        {
            var api = GoogleMusicWebClientInstance.GetInstance();

            var tracksTask = api.GetAllSongs();
            tracksTask.Wait();
            var tracks = tracksTask.Result;

            Assert.IsTrue(tracks.Any());
        }
    }
}
