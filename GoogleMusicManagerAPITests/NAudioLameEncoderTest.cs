using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleMusicManagerAPI.TrackSampleEncoder;

namespace GoogleMusicManagerAPITests
{
    [TestClass]
    public class NAudioLameEncoderTest : EncoderTestBase
    {
        [TestMethod]
        public void Test_NAudioLame_Sample_Creation()
        {
            var encoder = new NAudioLameEncoder();

            Test_Sample_Creation(encoder);
        }
    }
}
