using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.Messages
{
    public class UploadSessionRequest
    {
        public String protocolVersion { get; set; }
        public String clientId { get; set; }
        public CreateSessionRequest createSessionRequest { get; set; }

        public class CreateSessionRequest
        {
            public IEnumerable<Field> fields { get; set; }

            public interface Field
            {
            }

            public class External : Field
            {
                public External(string name, string filename)
                {
                    this.external = new ExternalFields()
                    {
                        name = name,
                        filename = filename,
                        put = new object(),
                    };
                }

                public ExternalFields external { get; set; }

                public class ExternalFields
                {
                    public string name { get; set; }
                    public object put { get; set; }
                    public string filename { get; set; }
                }
            }

            public class Inlined : Field
            {
                public Inlined(string name, string content)
                {
                    this.inlined = new InlinedFields()
                    {
                        name = name,
                        content = content,
                    };
                }
                public InlinedFields inlined { get; set; }

                public class InlinedFields
                {
                    public string name { get; set; }
                    public string content { get; set; }
                }
            }
        }
    }
}
