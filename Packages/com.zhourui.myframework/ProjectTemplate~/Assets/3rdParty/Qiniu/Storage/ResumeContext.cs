using Newtonsoft.Json;
using System.Collections.Generic;

namespace Qiniu.Storage
{
    /// <summary>
    /// 分片上传的上下文信息
    /// </summary>
    public class ResumeContext
    {
        /// <summary>
        /// 上下文信息
        /// </summary>
        [JsonProperty("ctx")]
        public string Ctx { get; set; }

        /// <summary>
        /// 校验和
        /// </summary>
        [JsonProperty("checksum")]
        public string Checksum { get; set; }

        /// <summary>
        /// crc32校验值
        /// </summary>
        [JsonProperty("crc32")]
        public uint Crc32 { get; set; }

        /// <summary>
        /// 文件偏移位置
        /// </summary>
        [JsonProperty("offset")]
        public long Offset { get; set; }

        /// <summary>
        /// 上传目的host
        /// </summary>
        [JsonProperty("host")]
        public string Host { get; set; }

        /// <summary>
        /// ctx失效时刻
        /// </summary>
        [JsonProperty("expired_at")]
        public long ExpiredAt { get; set; }

        /// <summary>
        /// 新版分片上传上下文etag
        /// </summary>
        [JsonProperty("etag")]
        public Dictionary<string, object> Etag { get; set; }

        /// <summary>
        /// 新版分片上传md5校验值
        /// </summary>
        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }
}
