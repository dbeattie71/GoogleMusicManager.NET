using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using GoogleMusicWebClientAPI.StreamingLoadAllTracks;

namespace GoogleMusicWebClientAPITest
{
    [TestClass]
    public class StreamingLoadAllTracksResponseProcessorTest
    {
        [TestMethod]
        public void StreamingLoadAllTracksProcessorTest()
        {
            var processor = new StreamingLoadAllTracksResponseProcessor();

            var response = File.ReadAllText("StreamingLoadAllTracks.html");

            var trackList = processor.ProcessStreamingLoadAllTracks(response);

            Assert.IsTrue(trackList.Any());
        }
    }
}
