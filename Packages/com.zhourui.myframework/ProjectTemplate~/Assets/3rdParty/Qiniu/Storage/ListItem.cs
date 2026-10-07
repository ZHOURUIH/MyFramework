using Newtonsoft.Json;
namespace Qiniu.Storage
{
    /// <summary>
    /// 文件描述
    /// </summary>
    public class ListItem
    {
        /// <summary>
        /// 文件名
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// 文件hash(ETAG)
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// 文件大小(字节)
        /// </summary>
        [JsonProperty("fsize")]
        public long Fsize { get; set; }

        /// <summary>
        /// 文件MIME类型
        /// </summary>
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [JsonProperty("putTime")]
        public long PutTime { get; set; }

        /// <summary>
        /// 文件存储类型
        /// 0 标准存储
        /// 1 低频存储
        /// 2 归档存储
        /// 3 深度归档存储
        /// 4 归档直读存储
        /// </summary>
        [JsonProperty("type")]
        public int FileType { get; set; }

        /// <summary>
        /// 资源内容的唯一属主标识
        /// 详见上传策略：https://developer.qiniu.com/kodo/1206/put-policy
        /// </summary>
        [JsonProperty("endUser", NullValueHandling = NullValueHandling.Ignore)]
        public string EndUser { get; set; }
        
        /// <summary>
        /// 文件的存储状态
        /// 0 启用
        /// 1 禁用
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
        
        /// <summary>
        /// 文件的 md5 值
        /// 服务端不确保一定返回此字段，详见：
        /// https://developer.qiniu.com/kodo/1284/list#:~:text=%E3%80%82%0A%0A%E7%B1%BB%E5%9E%8B%EF%BC%9A%E6%95%B0%E5%AD%97-,md5,-%E5%90%A6
        /// </summary>
        [JsonProperty("md5", NullValueHandling = NullValueHandling.Ignore)]
        public string Md5 { get; set; }
    }
}
