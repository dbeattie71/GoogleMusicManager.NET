using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class GetStreamUrlTest
    {
        [TestMethod]
        public void Test_GetStreamUrl()
        {
            var api = GoogleMusicWebClientInstance.GetInstance();

            var tracksTask = api.GetAllSongs();
            tracksTask.Wait();
            var tracks = tracksTask.Result;

            if (tracks.Any() == false)
            {
                Assert.Fail("No tracks to stream");
            }

            var streamingUrlTask = api.GetStreamURL(tracks.First());
            streamingUrlTask.Wait();
            var streamingUrl = streamingUrlTask.Result;

            Uri uriResult;
            var isValidUrl = Uri.TryCreate(streamingUrl, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            Assert.IsTrue(isValidUrl, "Is invalid url " + streamingUrl);

            var wc = new WebClient();
            var streamedTrack = wc.DownloadData(streamingUrl);
            Assert.IsTrue(streamedTrack.Length > 0);
        }
    }
}
