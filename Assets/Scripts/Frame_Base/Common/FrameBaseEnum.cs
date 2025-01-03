using UnityEngine;

// 加载资源的来源
public enum LOAD_SOURCE : byte
{
	ASSET_DATABASE,         // 从AssetDatabase加载
	ASSET_BUNDLE,           // 从AssetBundle加载
}

// 窗口模式类型
public enum WINDOW_MODE : byte
{
	[EnumLabel("窗口"), Tooltip("带边框的窗口模式")]
	WINDOWED,                       // 带边框的窗口模式
	[EnumLabel("全屏"), Tooltip("全屏模式")]
	FULL_SCREEN,                    // 全屏模式
	[EnumLabel("无边框"), Tooltip("无边框窗口模式")]
	NO_BOARD_WINDOW,                // 无边框窗口模式
	[EnumLabel("自定义全屏"), Tooltip("全屏并且使用下面设置的分辨率")]
	FULL_SCREEN_CUSTOM_RESOLUTION,  // 全屏并且使用下面设置的分辨率
}

// 日志等级
public enum LOG_LEVEL : byte
{
	NONE,       // 无效值
	LOW,        // 低
	NORMAL,     // 正常
	HIGH,       // 高
	FORCE,      // 强制显示
}

// 资源加载路径的选择方式
public enum ASSET_READ_PATH : byte
{
	NONE,                   // 无效值
	SAME_TO_REMOTE,         // 读取与远端一致的文件
	STREAMING_ASSETS_ONLY,  // 只从StreamingAssets中读取
}