using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI
{
    public interface ISessionStorage
    {
        void SetSerializedValue(string name, string value, bool unknown);
        string GetSerializedStringValue(string name, bool unknown);
    }
}
