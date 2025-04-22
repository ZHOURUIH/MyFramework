
// 更新游戏执行的阶段
public enum PROGRESS_TYPE : byte
{
	CHECKING_UPDATE,    // 检查更新
	DELETE_FILE,        // 删除本地无用文件
	DOWNLOAD_RESOURCE,  // 正在下载资源文件
	FINISH,             // 更新完毕
}

// 下载提示
public enum DOWNLOAD_TIP : byte
{
	NONE,                       // 无效值
	CHECKING_UPDATE,            // 正在检查更新
	DOWNLOAD_FAILED,            // 文件下载失败
	NOT_IN_REMOTE_FILE_LIST,    // 已经下载的文件不存在于远端的文件列表中,一般不会有这个错误
	VERIFY_FAILED,              // 文件校验失败
}