using Newtonsoft.Json;

namespace Qiniu.Storage
{
    /// <summary>
    /// 上传策略
    /// 参考文档：https://developer.qiniu.com/kodo/manual/1206/put-policy
    /// </summary>
    public class PutPolicy
    {
        /// <summary>
        /// [必需]bucket或者bucket:key
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// [可选]若为 1，表示允许用户上传以 scope 的 keyPrefix 为前缀的文件。
        /// </summary>
        [JsonProperty("isPrefixalScope", NullValueHandling = NullValueHandling.Ignore)]
        public int? isPrefixalScope { get; set; }

        /// <summary>
        /// [必需]上传策略失效时刻，请使用SetExpire来设置它
        /// </summary>
        [JsonProperty("deadline")]
        public long Deadline { get; private set; }

        /// <summary>
        /// [可选]"仅新增"模式
        /// </summary>
        [JsonProperty("insertOnly", NullValueHandling = NullValueHandling.Ignore)]
        public int? InsertOnly { get; set; }

        /// <summary>
        /// [可选]saveKey 的优先级设置。为 true 时，saveKey不能为空，会忽略客户端指定的key，强制使用saveKey进行文件命名。
        /// 默认为 false
        /// </summary>
        [JsonProperty("forceSaveKey", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceSaveKey { get; set; }

        /// <summary>
        /// [可选]保存文件的key
        /// </summary>
        [JsonProperty("saveKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SaveKey { get; set; }

        /// <summary>
        /// [可选]终端用户
        /// </summary>
        [JsonProperty("endUser", NullValueHandling = NullValueHandling.Ignore)]
        public string EndUser { get; set; }

        /// <summary>
        /// [可选]返回URL
        /// </summary>
        [JsonProperty("returnUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ReturnUrl { get; set; }

        /// <summary>
        /// [可选]返回内容
        /// </summary>
        [JsonProperty("returnBody", NullValueHandling = NullValueHandling.Ignore)]
        public string ReturnBody { get; set; }

        /// <summary>
        /// [可选]回调URL
        /// </summary>
        [JsonProperty("callbackUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// [可选]回调内容
        /// </summary>
        [JsonProperty("callbackBody", NullValueHandling = NullValueHandling.Ignore)]
        public string CallbackBody { get; set; }

        /// <summary>
        /// [可选]回调内容类型
        /// </summary>
        [JsonProperty("callbackBodyType", NullValueHandling = NullValueHandling.Ignore)]
        public string CallbackBodyType { get; set; }

        /// <summary>
        /// [可选]回调host
        /// </summary>
        [JsonProperty("callbackHost", NullValueHandling = NullValueHandling.Ignore)]
        public string CallbackHost { get; set; }

        /// <summary>
        /// [可选]回调fetchkey
        /// </summary>
        [JsonProperty("callbackFetchKey", NullValueHandling = NullValueHandling.Ignore)]
        public int? CallbackFetchKey { get; set; }

        /// <summary>
        /// [可选]上传预转持久化，与 PersistentWorkflowTemplateId 二选一
        /// </summary>
        [JsonProperty("persistentOps", NullValueHandling = NullValueHandling.Ignore)]
        public string PersistentOps { get; set; }

        /// <summary>
        /// [可选]持久化结果通知
        /// </summary>
        [JsonProperty("persistentNotifyUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string PersistentNotifyUrl { get; set; }

        /// <summary>
        /// [可选]私有队列
        /// </summary>
        [JsonProperty("persistentPipeline", NullValueHandling = NullValueHandling.Ignore)]
        public string PersistentPipeline { get; set; }

        /// <summary>
        /// [可选]持久化任务类型，为 1 时开启闲时任务
        /// </summary>
        [JsonProperty("persistentType", NullValueHandling = NullValueHandling.Ignore)]
        public int? PersistentType { get; set; }
        
        /// <summary>
        /// [可选]任务模版，与 PersistentOps 二选一
        /// </summary>
        [JsonProperty("persistentWorkflowTemplateID", NullValueHandling = NullValueHandling.Ignore)]
        public string PersistentWorkflowTemplateId { get; set; }


        /// <summary>
        /// [可选]上传文件大小限制：最小值，单位Byte
        /// </summary>
        [JsonProperty("fsizeMin", NullValueHandling = NullValueHandling.Ignore)]
        public long? FsizeMin { get; set; }

        /// <summary>
        /// [可选]上传文件大小限制：最大值，单位Byte
        /// </summary>
        [JsonProperty("fsizeLimit", NullValueHandling = NullValueHandling.Ignore)]
        public long? FsizeLimit { get; set; }

        /// <summary>
        /// [可选]上传时是否自动检测MIME
        /// </summary>
        [JsonProperty("detectMime", NullValueHandling = NullValueHandling.Ignore)]
        public int? DetectMime { get; set; }

        /// <summary>
        /// [可选]上传文件MIME限制
        /// </summary>
        [JsonProperty("mimeLimit", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeLimit { get; set; }

        /// <summary>
        /// [可选]文件上传后多少天后自动删除
        /// </summary>
        [JsonProperty("deleteAfterDays", NullValueHandling = NullValueHandling.Ignore)]
        public int? DeleteAfterDays { get; set; }

        /// <summary>
        /// [可选]文件的存储类型，默认为普通存储，设置为：0 标准存储（默认），1 低频存储，2 归档存储，3 深度归档存储
        /// </summary>
        [JsonProperty("fileType", NullValueHandling = NullValueHandling.Ignore)]
        public int? FileType { get; set; }

        /// <summary>
        /// 设置上传凭证有效期（配置Deadline属性）
        /// </summary>
        /// <param name="expireInSeconds"></param>
        public void SetExpires(int expireInSeconds)
        {
            this.Deadline = Util.UnixTimestamp.GetUnixTimestamp(expireInSeconds);
        }

        public void SetExpires(long expireInSeconds)
        {
            this.Deadline = Util.UnixTimestamp.GetUnixTimestamp(expireInSeconds);
        }

        /// <summary>
        /// 转换为JSON字符串
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string ToJsonString()
        {
            if (this.Deadline == 0)
            {
                //默认一个小时有效期
                this.SetExpires(3600);
            }
            return JsonConvert.SerializeObject(this);
        }

    }
}
