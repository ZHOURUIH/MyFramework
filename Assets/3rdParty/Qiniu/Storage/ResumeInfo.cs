using Newtonsoft.Json;
using System.Collections.Generic;
namespace Qiniu.Storage
{
    /// <summary>
    /// 分片上传的记录信息
    /// </summary>
    public class ResumeInfo
    {
        /// <summary>
        /// 文件大小
        /// </summary>
        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// 文件块总数
        /// </summary>
        [JsonProperty("blockCount")]
        public long BlockCount { get; set; }

        /// <summary>
        /// 上下文信息列表
        /// </summary>
        [JsonProperty("contexts")]
        public string[] Contexts { get; set; }

        /// <summary>
        /// 上下文信息过期列表，与 context 配合使用
        /// </summary>
        [JsonProperty("contextsExpiredAt")]
        public long[] ContextsExpiredAt { get; set; }

        /// <summary>
        /// Ctx过期时间戳（单位秒）
        /// </summary>
        [JsonProperty("expiredAt")]
        public long ExpiredAt { get; set; }

        /// <summary>
        /// 上传进度信息序列化
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// 新版分片上下文信息列表
        /// </summary>
        [JsonProperty("etags")]
        public Dictionary<string, object>[] Etags { get; set; }

        /// <summary>
        /// 新版分片上传id
        /// </summary>
        [JsonProperty("uploadId")]
        public string UploadId { get; set; }

        /// <summary>
        /// 完成上传的字节数
        /// </summary>
        [JsonProperty("uploaded")]
        public long Uploaded { get; set; }

        public string ToJsonStr()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
