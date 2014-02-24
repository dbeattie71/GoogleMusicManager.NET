using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackSampleEncoder
{
    public class AVConvEncoder : ITrackSampleEncoder
    {
        private string pathToExe = string.Empty;
        public AVConvEncoder()
        {
        }

        public AVConvEncoder(string pathToExe)
        {
            this.pathToExe = pathToExe ?? string.Empty;
        }

        public byte[] GetMP3Sample(string filename, int start, int duration)
        {
            var tempFileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(filename) + "_sample" + ".mp3";
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            var argumentTemplate = "-i \"{0}\" -t {1} -ss {2} -ab 128k -f s16le -c libmp3lame \"{3}\"";

            var populatedArguments = string.Format(argumentTemplate, filename, duration, start, tempFileName);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(pathToExe, "avconv.exe"),
                    Arguments = populatedArguments,
                    WindowStyle = ProcessWindowStyle.Minimized,
                }
            };
            process.Start();
            process.WaitForExit();

            var bytes = File.ReadAllBytes(tempFileName);
            File.Delete(tempFileName);
            return bytes;
        }

    }
}
