using System.Collections.Generic;

namespace Qiniu.Storage
{
    /// <summary>
    /// 文件上传的额外可选设置
    /// </summary>
    public class PutExtra
    {
        /// <summary>
        /// 设置文件断点续传进度记录文件
        /// </summary>
        public string ResumeRecordFile { set; get; }
        /// <summary>
        /// 上传可选参数字典，参数名次以 x: 开头
        /// </summary>
        public Dictionary<string, string> Params;
        /// <summary>
        /// 指定文件的MimeType
        /// </summary>
        public string MimeType { set; get; }
		/// <summary>
		/// 指定文件下载时的缓存行为
		/// </summary>
		public string CacheControl { set; get; }
		/// <summary>
		/// 设置文件上传进度处理器
		/// </summary>
		public UploadProgressHandler ProgressHandler { set; get; }
        /// <summary>
        /// 设置文件上传的状态控制器
        /// </summary>
        public UploadController UploadController { set; get; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        /// 默认值应与 <see cref="Config.MaxRetryTimes"/> 一致
        public int MaxRetryTimes { set; get; } = 3;

        /// <summary>
        /// 块并发上传的线程数量
        /// </summary>
        public int BlockUploadThreads { set; get; }

        /// <summary>
        /// 分片上传版本 目前支持v1/v2版本 默认v1
        /// </summary>
        public string Version = "v1";

        /// <summary>
        /// 分片上传v2字段 默认大小为4MB 分片大小范围为1 MB - 1 GB
        /// </summary>
        public int PartSize = 4 * 1024 * 1024;
}
}