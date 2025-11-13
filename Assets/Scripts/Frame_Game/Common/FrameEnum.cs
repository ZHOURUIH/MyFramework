
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

// x方向上要停靠的边界
public enum HORIZONTAL_PADDING : sbyte
{
	NONE = -1,      // 无效值
	LEFT_IN,        // 向左并且位于父节点内停靠
	LEFT_OUT,       // 向左并且位于父节点外停靠
	RIGHT_IN,       // 向右并且位于父节点内停靠
	RIGHT_OUT,      // 向右并且位于父节点外停靠
	CENTER,         // 以中间停靠
}

// y方向上要停靠的边界
public enum VERTICAL_PADDING : sbyte
{
	NONE = -1,      // 无效值
	TOP_IN,         // 向上并且位于父节点内停靠
	TOP_OUT,        // 向上并且位于父节点外停靠
	BOTTOM_IN,      // 向下并且位于父节点内停靠
	BOTTOM_OUT,     // 向下并且位于父节点外停靠
	CENTER,         // 以中间停靠
}