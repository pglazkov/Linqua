using Newtonsoft.Json;

namespace Linqua.Service.DataObjects
{
    public class LogUploadInfo
    {
        [JsonProperty(PropertyName = "containerName")]
        public string ContainerName { get; set; }

        [JsonProperty(PropertyName = "resourceName")]
        public string ResourceName { get; set; }

        [JsonProperty(PropertyName = "sasQueryString")]
        public string SasQueryString { get; set; }

        [JsonProperty(PropertyName = "uploadUri")]
        public string UploadUri { get; set; }
    }
}