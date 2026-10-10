namespace Qiniu.Storage
{
    /// <summary>
    /// 目前已支持的区域：华东/华东2/华北/华南/北美/新加坡/首尔
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// 资源管理
        /// </summary>
        public string RsHost { set; get; }

        /// <summary>
        /// 源列表
        /// </summary>
        public string RsfHost { set; get; }

        /// <summary>
        /// 数据处理
        /// </summary>
        public string ApiHost { set; get; }

        /// <summary>
        /// 镜像刷新、资源抓取
        /// </summary>
        public string IovipHost { set; get; }

        /// <summary>
        /// 资源上传
        /// </summary>
        public string[] SrcUpHosts { set; get; }

        /// <summary>
        /// CDN加速
        /// </summary>
        public string[] CdnUpHosts { set; get; }

        /// <summary>
        /// 华东
        /// </summary>
        public static Zone ZONE_CN_East = new Zone()
        {
            RsHost = "rs.qbox.me",
            RsfHost = "rsf.qbox.me",
            ApiHost = "api.qiniuapi.com",
            IovipHost = "iovip.qbox.me",
            SrcUpHosts = new string[] { "up.qiniup.com" },
            CdnUpHosts = new string[] { "upload.qiniup.com" }
        };

        /// <summary>
        /// 华东-浙江2
        /// </summary>
        public static Zone ZONE_CN_East_2 = new Zone()
        {
            RsHost = "rs-cn-east-2.qiniuapi.com",
            RsfHost = "rsf-cn-east-2.qiniuapi.com",
            ApiHost = "api-cn-east-2.qiniuapi.com",
            IovipHost = "iovip-cn-east-2.qiniuio.com",
            SrcUpHosts = new string[] { "up-cn-east-2.qiniup.com" },
            CdnUpHosts = new string[] { "upload-cn-east-2.qiniup.com" }
        };

        /// <summary>
        /// 华北
        /// </summary>
        public static Zone ZONE_CN_North = new Zone()
        {
            RsHost = "rs-z1.qbox.me",
            RsfHost = "rsf-z1.qbox.me",
            ApiHost = "api-z1.qiniuapi.com",
            IovipHost = "iovip-z1.qbox.me",
            SrcUpHosts = new string[] { "up-z1.qiniup.com" },
            CdnUpHosts = new string[] { "upload-z1.qiniup.com" }
        };

        /// <summary>
        /// 华南
        /// </summary>
        public static Zone ZONE_CN_South = new Zone()
        {
            RsHost = "rs-z2.qbox.me",
            RsfHost = "rsf-z2.qbox.me",
            ApiHost = "api-z2.qiniuapi.com",
            IovipHost = "iovip-z2.qbox.me",
            SrcUpHosts = new string[] { "up-z2.qiniup.com" },
            CdnUpHosts = new string[] { "upload-z2.qiniup.com" }
        };

        /// <summary>
        /// 北美
        /// </summary>
        public static Zone ZONE_US_North = new Zone()
        {
            RsHost = "rs-na0.qbox.me",
            RsfHost = "rsf-na0.qbox.me",
            ApiHost = "api-na0.qiniuapi.com",
            IovipHost = "iovip-na0.qbox.me",
            SrcUpHosts = new string[] { "up-na0.qiniup.com" },
            CdnUpHosts = new string[] { "upload-na0.qiniup.com" }
        };

        /// <summary>
        /// 新加坡
        /// </summary>
        public static Zone ZONE_AS_Singapore = new Zone()
        {
            RsHost = "rs-as0.qbox.me",
            RsfHost = "rsf-as0.qbox.me",
            ApiHost = "api-as0.qiniuapi.com",
            IovipHost = "iovip-as0.qbox.me",
            SrcUpHosts = new string[] { "up-as0.qiniup.com" },
            CdnUpHosts = new string[] { "upload-as0.qiniup.com" }
        };
    }
}
