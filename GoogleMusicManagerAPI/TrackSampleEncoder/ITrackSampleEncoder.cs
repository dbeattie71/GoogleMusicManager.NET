using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.TrackSampleEncoder
{
    public interface ITrackSampleEncoder
    {
        byte[] GetMP3Sample(string filename, int start, int duration);
    }
}
