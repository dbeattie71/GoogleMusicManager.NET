using System;
using System.IO;

namespace GoogleMusicManagerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileList = Directory.GetFiles(@"D:\Testmp3", "*.mp3", SearchOption.AllDirectories);
            var uploader = new UploadProcess();
            uploader.DoUpload(fileList);
            Console.ReadKey();
        }


    }

}
