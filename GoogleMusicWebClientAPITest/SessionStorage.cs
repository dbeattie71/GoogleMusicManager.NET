using GoogleMusicWebClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPITest
{
    class SessionStorage : ISessionStorage
    {
        private Dictionary<string, string> Storage { get; set; }
        public SessionStorage()
        {
            this.Storage = new Dictionary<string, string>();
        }

        public void SetSerializedValue(string name, string value, bool unknown)
        {
            if (this.Storage.ContainsKey(name))
            {
                this.Storage[name] = value;
            }
            else
            {
                this.Storage.Add(name, value);
            }
        }

        public string GetSerializedStringValue(string name, bool unknown)
        {
            if (this.Storage.ContainsKey(name))
            {
                return this.Storage[name];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
