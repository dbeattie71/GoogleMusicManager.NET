using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Byteopia.Helpers
{

    public class JSON
    {
        public static T DeserializeObject<T>(String data)
        {
            return Deserialize<T>(data);
        }

        public static T Deserialize<T>(String data)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(ms);
        }

        public static string SerializeObject(object o)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(o.GetType());
            MemoryStream ms = new MemoryStream();

            serializer.WriteObject(ms, o);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }
    }
}