using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using GoogleMusicWebClientAPI.RecommendedForYou;
using System.Linq;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class RecommendedForYouResponseProcessorTest
    {
        [TestMethod]
        public void Test_RecommendedForYouResponseProcessor()
        {
            var processor = new RecommendedForYouResponseProcessor();

            var response = File.ReadAllText("recommendedforyou.html");

            var trackList = processor.RecommendedForYouResponse(response);

            Assert.IsTrue(trackList.Any());
        }
    }
}
