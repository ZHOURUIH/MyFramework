namespace Qiniu.Http
{
    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public enum HttpCode
    {
        #region _PRE_

        /// <summary>
        /// 成功
        /// </summary>
        OK = 200,

        /// <summary>
        /// 部分OK
        /// </summary>
        PARTLY_OK = 298,

        /// <summary>
        /// 请求错误
        /// </summary>
        BAD_REQUEST = 400,

        /// <summary>
        /// 认证授权失败
        /// </summary>
        AUTHENTICATION_FAILED = 401,

        /// <summary>
        /// 拒绝访问
        /// </summary>
        ACCESS_DENIED = 403,

        /// <summary>
        /// 资源不存在
        /// </summary>
        OBJECT_NOT_FOUND = 404,

        /// <summary>
        /// CRC32校验失败
        /// </summary>
        CRC32_CHECK_FAILEd = 406,

        /// <summary>
        /// 上传文件大小超限
        /// </summary>
        FILE_SIZE_EXCEED = 413,

        /// <summary>
        /// 镜像回源失败
        /// </summary>
        PREFETCH_FAILED = 478,
        
        /// <summary>
        /// 接口未实现
        /// </summary>
        NOT_IMPLEMENTED = 501,

        /// <summary>
        /// 错误网关
        /// </summary>
        BAD_GATEWAY = 502,

        /// <summary>
        /// 服务端不可用
        /// </summary>
        SERVER_UNAVAILABLE = 503,

        /// <summary>
        /// 服务端操作超时
        /// </summary>
        SERVER_TIME_EXCEED = 504,

        /// <summary>
        /// 超出带宽限制
        /// </summary>
        BANDWIDTH_LIMIT_EXCEEDED = 509,
        
        /// <summary>
        /// 单个资源访问频率过高
        /// </summary>
        TOO_FREQUENT_ACCESS = 573,

        /// <summary>
        /// 回调失败
        /// </summary>
        CALLBACK_FAILED = 579,

        /// <summary>
        /// 服务端操作失败
        /// </summary>
        SERVER_OPERATION_FAILED = 599,

        /// <summary>
        /// 资源内容被修改
        /// </summary>
        CONTENT_MODIFIED = 608,

        /// <summary>
        /// 文件不存在/指定资源不存在或已被删除
        /// </summary>
        FILE_NOT_EXIST = 612,

        /// <summary>
        /// 文件已存在/目标资源已存在
        /// </summary>
        FILE_EXISTS = 614,

        /// <summary>
        /// 当前操作无法在共享空间执行（被共享的用户进行操作时）
        /// </summary>
        INVALID_SHARE_BUCKET = 616,

        /// <summary>
        /// 当前操作无法在共享空间执行（所有者进行操作时）
        /// </summary>
        BUCKET_IS_SHARING = 618,

        /// <summary>
        /// 空间数量已达上限
        /// </summary>
        BUCKET_COUNT_LIMIT = 630,

        /// <summary>
        /// 空间或者文件不存在
        /// </summary>
        BUCKET_NOT_EXIST = 631,

        /// <summary>
        /// 共享空间达到上限
        /// </summary>
        EXCEED_SHARED_BUCKETS_LIMIT = 632,

        /// <summary>
        /// 列举资源(list)使用了非法的marker
        /// </summary>
        INVALID_MARKER = 640,

        /// <summary>
        /// 在断点续上传过程中，后续上传接收地址不正确或ctx信息已过期。
        /// </summary>
        CONTEXT_EXPIRED = 701,

        #endregion _PRE_

        #region _USR_

        /// <summary>
        /// 自定义HTTP状态码 (默认值)
        /// </summary>
        USER_UNDEF = 0,

        /// <summary>
        /// 自定义HTTP状态码 (用户取消)
        /// </summary>
        USER_CANCELED = -2,

        /// <summary>
        /// 自定义HTTP状态码 (用户暂停)
        /// </summary>
        USER_PAUSED = 1,

        /// <summary>
        /// 自定义HTTP状态码 (用户继续)
        /// </summary>
        USER_RESUMED = 2,

        /// <summary>
        /// 自定义HTTP状态码 (需要重试)
        /// </summary>
        USER_NEED_RETRY = 3,

        /// <summary>
        /// 自定义HTTP状态码 (异常或错误)
        /// </summary>
        INVALID_ARGUMENT = -4,

        /// <summary>
        /// 自定义HTTP状态码（文件不合法）
        /// </summary>
        INVALID_FILE = -3,

        /// <summary>
        /// 自定义HTTP状态码（凭证不合法）
        /// </summary>
        INVALID_TOKEN = -5,

        #endregion _USR_

    }

}
