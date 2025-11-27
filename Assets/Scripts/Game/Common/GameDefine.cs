using System.Collections.Generic;

// 游戏常量定义
public class GameDefine
{
	// 路径定义
	// 常量定义
	public const string ANDROID_PLUGIN_BUNDLE_NAME = "com.your.packagename";		// 安卓插件的包名
	//-----------------------------------------------------------------------------------------------------------------
	// 标签
	// 层

	// 允许动态下载的目录列表,此列表中的文件不会打包到apk中,也不会在游戏启动时从服务器下载,而是在加载资源时才会进行下载
	// 这里不要写成一行,需要换行写,才能正确解析
	public static List<string> DYNAMIC_DOWNLOAD_LIST = new()
	{
		"DynamicDownloading/",
	};
}