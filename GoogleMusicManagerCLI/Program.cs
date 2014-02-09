using System;
using System.IO;

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
                var fileList = Directory.GetFiles(path, "*.mp3", options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                var uploader = new UploadProcess();
                uploader.DoUpload(fileList);
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }

}
