using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.Messages
{
    public class UploadSessionResponse
    {
        public SessionStatus sessionStatus { get; set; }

        public class SessionStatus
        {
            public string state { get; set; }
            public ExternalFieldTransfer[] externalFieldTransfers { get; set; }
            public string upload_id { get; set; }

            public class ExternalFieldTransfer
            {
                public string name { get; set; }
                public string status { get; set; }
                public int bytesTransferred { get; set; }
                public PutInfo putInfo { get; set; }
                public string content_type { get; set; }
                public class PutInfo
                {
                    public string url { get; set; }
                }
            }
        }

        public ErrorMessage errorMessage { get; set; }

        public class ErrorMessage
        {
            public string reason { get; set; }
            public AdditionalInfo additionalInfo { get; set; }
            public string upload_id { get; set; }
        }

    }
}
