using GoogleMusicManagerAPI;
using GoogleMusicManagerAPI.TrackMetadata;
using GoogleMusicManagerAPI.TrackSampleEncoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GoogleMusicManagerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var path = options.Path;

                var extensions = new List<string>()
                {
                    "*.mp3",
                    "*.wma",
                    "*.m4a",
                };

                var fileList = extensions.Select(p => Directory.GetFiles(path, p, options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).SelectMany(p => p);

                var trackMetadataFacade = new TrackMetadataFacade();
                var trackMetadataList = fileList.Select(p => trackMetadataFacade.CreateTrackMetadata(p));

                var uploader = new UploadProcess(
                    new OauthTokenStorage(options.OauthFile), 
                    new UploadProcessObserver(),
                    new AVConvEncoder(ConfigurationManager.AppSettings["avconvpath"])
                    );
                var uploaderTask = uploader.DoUpload(trackMetadataList);
                uploaderTask.Wait();
                var success = uploaderTask.Result;
            }
        }
    }

}
