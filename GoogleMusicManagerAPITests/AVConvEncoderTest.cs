﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Configuration;
using GoogleMusicManagerAPI.TrackSampleEncoder;

namespace GoogleMusicManagerAPITests
{
    [TestClass]
    public class AVConvEncoderTest : EncoderTestBase
    {
        [TestMethod]
        public void Test_AVConv_Sample_Creation()
        {
            var encoder = new AVConvEncoder(ConfigurationManager.AppSettings["avconvpath"]);

            Test_Sample_Creation(encoder);
        }




    }
}
