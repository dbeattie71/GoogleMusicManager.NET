using GoogleMusicManagerAPI.TrackSampleEncoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPITests
{
    public class EncoderTestBase
    {
        protected void Test_Sample_Creation(ITrackSampleEncoder encoder)
        {
            var source = @"http://sampleswap.org/mp3/download.php?song=1122&q=lofi";

            var fullTrack = "fullTrack.mp3";

            DownloadFileIfNotExists(source, fullTrack);


            var encodedBytes = encoder.GetMP3Sample(fullTrack, 45, 15);

            var sample = "sample.mp3";
            File.WriteAllBytes(sample, encodedBytes);

            var tagLibFile = TagLib.File.Create(sample);
            var tags = tagLibFile.Tag;
            var properties = tagLibFile.Properties;

            Assert.AreEqual(128, properties.AudioBitrate);
            Assert.AreEqual(15, (int)properties.Duration.TotalSeconds);
        }

        private void DownloadFileIfNotExists(string source, string fullTrack)
        {
            if (File.Exists(fullTrack) == false)
            {
                var webclient = new WebClient();
                webclient.DownloadFile(source, fullTrack);
            }
        }

    }
}
