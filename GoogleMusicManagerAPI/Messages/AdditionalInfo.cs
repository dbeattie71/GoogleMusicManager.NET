using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerAPI.Messages
{
    public class AdditionalInfo
    {
        [JsonProperty("uploader_service.GoogleRupioAdditionalInfo")]
        public GoogleRupioAdditionalInfo googleRupioAdditionalInfo { get; set; }
        public class GoogleRupioAdditionalInfo
        {
            public CompletionInfo completionInfo { get; set; }
            public class CompletionInfo
            {
                public string status { get; set; }
                public CustomerSpecificInfo customerSpecificInfo { get; set; }

                public class CustomerSpecificInfo
                {
                    public string ServerFileReference { get; set; }
                    public int ResponseCode { get; set; }
                }
            }

            public RequestRejectedInfo requestRejectedInfo { get; set; }
            public class RequestRejectedInfo
            {
                public string reasonDescription { get; set; }
            }
        }

    }
}
