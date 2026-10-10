using Newtonsoft.Json;

namespace Qiniu.Storage
{
    /// <summary>
    /// 从uc.qbox.me返回的消息
    /// </summary>
    internal class ZoneInfo
    {
        [JsonProperty("hosts")]
        public ZoneHost[] Hosts { get; set; }
    }

    internal class ZoneHost
    {
        /// <summary>
        /// 过期时间，单位：秒
        /// </summary>
        [JsonProperty("ttl")]
        public int Ttl { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("io")]
        public ServiceDomains Io { get; set; }

        [JsonProperty("io_src")]
        public ServiceDomains IoSrc { get; set; }

        [JsonProperty("up")]
        public ServiceDomains Up { get; set; }
        
        [JsonProperty("api")]
        public ServiceDomains Api { get; set; }
        
        [JsonProperty("rs")]
        public ServiceDomains Rs { get; set; }
        
        [JsonProperty("rsf")]
        public ServiceDomains Rsf { get; set; }
        
        [JsonProperty("s3")]
        public ServiceDomains S3 { get; set; }
        
        [JsonProperty("uc")]
        public ServiceDomains Uc { get; set; }

        internal class ServiceDomains
        {
            [JsonProperty("domains")]
            public string[] Domains { get; set; }

            [JsonProperty("old", NullValueHandling = NullValueHandling.Ignore)]
            public string[] Old { get; set; }

            [JsonProperty("region_alias", NullValueHandling = NullValueHandling.Ignore)]
            public string RegionAlias { get; set; }
        }
    }
}
