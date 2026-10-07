using System;
using Qiniu.Util;

namespace Qiniu.Http
{
    /// <summary>
    /// HTTP辅助工具:帮助生成UA,boundary等
    /// </summary>
    public class HttpHelper
    {
        /// <summary>
        /// 资源类型：普通文本
        /// </summary>
        public static string CONTENT_TYPE_TEXT_PLAIN = "text/plain";

        /// <summary>
        /// 资源类型：JSON字符串
        /// </summary>
        public static string CONTENT_TYPE_APP_JSON = "application/json";

        /// <summary>
        /// 资源类型：未知类型(数据流)
        /// </summary>
        public static string CONTENT_TYPE_APP_OCTET = "application/octet-stream";

        /// <summary>
        /// 资源类型：表单数据(键值对)
        /// </summary>
        public static string CONTENT_TYPE_WWW_FORM = "application/x-www-form-urlencoded";

        /// <summary>
        /// 资源类型：多分部数据
        /// </summary>
        public static string CONTENT_TYPE_MULTIPART = "multipart/form-data";
        
        /// <summary>
        /// HTTP状态码200 (OK)
        /// </summary>
        public static int STATUS_CODE_OK = 200;

        /// <summary>
        /// HTTP状态码298 (部分OK)
        /// </summary>
        public static int STATUS_CODE_PARTLY_OK = 298;

        /// <summary>
        /// 自定义HTTP状态码 (默认值)
        /// </summary>
        public static int STATUS_CODE_UNDEF = -256;

        /// <summary>
        /// 自定义HTTP状态码 (用户取消)
        /// </summary>
        public static int STATUS_CODE_USER_CANCELED = -255;

        /// <summary>
        /// 自定义HTTP状态码 (用户暂停)
        /// </summary>
        public static int STATUS_CODE_USER_PAUSED = -254;

        /// <summary>
        /// 自定义HTTP状态码 (用户继续)
        /// </summary>
        public static int STATUS_CODE_USER_RESUMED = -253;

        /// <summary>
        /// 自定义HTTP状态码 (需要重试)
        /// </summary>
        public static int STATUS_CODE_NEED_RETRY= -252;

        /// <summary>
        /// 自定义HTTP状态码 (异常或错误)
        /// </summary>
        public static int STATUS_CODE_EXCEPTION = -252;

        /// <summary>
        /// 客户端标识
        /// </summary>
        /// <returns>客户端标识UA</returns>
        public static string getUserAgent()
        {
#if NetStandard
            string sfx = Environment.MachineName;
#else
            var osInfo = Environment.OSVersion;
            string sfx = Environment.MachineName + "; " + osInfo.Platform + "; " + osInfo.Version; 
#endif
            return string.Format("{0}/{1} ({2})", QiniuCSharpSDK.ALIAS, QiniuCSharpSDK.VERSION, sfx);
        }

        /// <summary>
        /// 多部分表单数据(multi-part form-data)的分界(boundary)标识
        /// </summary>
        /// <returns>多部分表单数据的boundary</returns>
        public static string createFormDataBoundary()
        {
            string now = DateTime.UtcNow.Ticks.ToString();
            return string.Format("-------{0}Boundary{1}", QiniuCSharpSDK.ALIAS, Hashing.CalcMD5(now));
        }
    }
}
