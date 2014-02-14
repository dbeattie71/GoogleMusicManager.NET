using GoogleMusicManagerAPI;
using System;
using System.Collections.Generic;
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
                };

                var fileList = extensions.Select(p => Directory.GetFiles(path, p, options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).SelectMany(p => p);

                var uploader = new UploadProcess(new OauthTokenStorage(options.OauthFile), new UploadProcessObserver());
                var uploaderTask = uploader.DoUpload(fileList);
                uploaderTask.Wait();
                var success = uploaderTask.Result;
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }

}
