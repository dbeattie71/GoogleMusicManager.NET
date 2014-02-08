using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.Messages
{
    public class JsonUploadResponse
    {
        public SessionStatus sessionStatus { get; set; }

        public class SessionStatus
        {
            public string state { get; set; }
            public ExternalFieldTransfer[] externalFieldTransfers { get; set; }

            public class ExternalFieldTransfer
            {
                public string name { get; set; }
                public string status { get; set; }
                public int bytesTransferred { get; set; }
                public int bytesTotal { get; set; }
                public PutInfo putInfo { get; set; }
                public string content_type { get; set; }

                public class PutInfo
                {
                    public string url { get; set; }
                }
            }

            public AdditionalInfo additionalInfo { get; set; }
            //public class AdditionalInfo
            //{
            //    [JsonProperty("uploader_service.GoogleRupioAdditionalInfo")]
            //    public GoogleRupioAdditionalInfo googleRupioAdditionalInfo { get; set; }
            //    public class GoogleRupioAdditionalInfo
            //    {
            //        public CompletionInfo completionInfo { get; set; }
            //        public class CompletionInfo
            //        {
            //            public string status { get; set; }
            //            public CustomerSpecificInfo customerSpecificInfo { get; set; }
            //            public class CustomerSpecificInfo
            //            {
            //                public string ServerFileReference { get; set; }
            //                public int ResponseCode { get; set; }
            //            }
            //        }
            //    }
            //}
            public string upload_id { get; set; }
        }
    }
}
