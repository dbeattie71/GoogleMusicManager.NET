using NAudio.FileFormats.Mp3;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackSampleEncoder
{
    public class NAudioLameEncoder : ITrackSampleEncoder
    {
        public byte[] GetMP3Sample(string filename, int start, int duration)
        {
            //var begin = new TimeSpan(0, 0, start);
            //var end = new TimeSpan(0, 0, start + duration);

            using (var reader = new Mp3FileReader(filename))
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new LameMP3FileWriter(ms, reader.WaveFormat, 128))
                    {
                        Mp3Frame frame;

                        IMp3FrameDecompressor decompressor = null;
                        var decompressed = new byte[100000];

                        while ((frame = reader.ReadNextFrame()) != null)
                        {
                            if (decompressor == null)
                            {
                                var waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                                decompressor = new DmoMp3FrameDecompressor(waveFormat);
                            }

                            var decompressedSize = decompressor.DecompressFrame(frame, decompressed, 0);

                            if (reader.CurrentTime.TotalSeconds >= start)
                            {
                                if (reader.CurrentTime.TotalSeconds <= start + duration)
                                {
                                    writer.Write(decompressed, 0, decompressedSize);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
