using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class RecommendedForYouTest
    {
        [TestMethod]
        public void Test_RecommendedForYou()
        {
            var api = GoogleMusicWebClientInstance.GetInstance();

            var tracksTask = api.GetRecommendedForYou();
            tracksTask.Wait();
            var tracks = tracksTask.Result;

            Assert.IsTrue(tracks.Any());

        }
    }
}
