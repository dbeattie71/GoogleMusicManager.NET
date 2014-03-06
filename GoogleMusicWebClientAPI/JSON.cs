using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Byteopia.Helpers
{

    public class JSON
    {
        public static T Deserialize<T>(String data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }


        public static string SerializeObject(object o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }
}