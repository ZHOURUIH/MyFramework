using UnityEngine;
#if UNITY_WEIXINMINIGAME
using WeChatWASM;
#endif
using static FrameBaseUtility;

public class FrameBaseDefine
{
	public const string ASSETS = "Assets";
	public const string ANDROID = "Android";
	public const string WINDOWS = "Windows";
	public const string WEBGL = "WebGL";
	public const string IOS = "iOS";
	public const string MACOS = "MacOS";
	public const string RESOURCES = "Resources";
	// 安卓真机上没有STREAMING_ASSETS的定义
#if UNITY_EDITOR
	public const string STREAMING_ASSETS = "StreamingAssets";
#elif UNITY_IOS
	public const string STREAMING_ASSETS = "Raw";
#elif !UNITY_ANDROID
	public const string STREAMING_ASSETS = "StreamingAssets";
#endif
	// Windows下才能使用临时目录
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	public static string F_TEMPORARY_CACHE_PATH = Application.temporaryCachePath + "/";
#endif
	public static string F_STREAMING_ASSETS_PATH = Application.streamingAssetsPath + "/";
	// 各个平台下的AssetBundle路径不一样,为了避免打包资源时冲突
#if UNITY_ANDROID
	public static string F_ASSET_BUNDLE_PATH = F_STREAMING_ASSETS_PATH + ANDROID + "/";
#elif UNITY_STANDALONE_WIN
	public static string F_ASSET_BUNDLE_PATH = F_STREAMING_ASSETS_PATH + WINDOWS + "/";
#elif UNITY_IOS
	public static string F_ASSET_BUNDLE_PATH = F_STREAMING_ASSETS_PATH + IOS + "/";
#elif UNITY_STANDALONE_OSX
	public static string F_ASSET_BUNDLE_PATH = F_STREAMING_ASSETS_PATH + MACOS + "/";
#elif UNITY_WEBGL
	public static string F_ASSET_BUNDLE_PATH = F_STREAMING_ASSETS_PATH + WEBGL + "/";
#endif
	// 绝对路径,以F_开头,表示Full
#if !UNITY_EDITOR && UNITY_WEBGL
#if BYTE_DANCE
	public static string F_PERSISTENT_DATA_PATH = GetTTPersistantPath();
#elif UNITY_WEIXINMINIGAME
	public static string F_PERSISTENT_DATA_PATH = WXBase.env.USER_DATA_PATH + "/";
#endif
#else
	public static string F_PERSISTENT_DATA_PATH = Application.persistentDataPath + "/";
#endif
	public static string F_PERSISTENT_ASSETS_PATH = F_PERSISTENT_DATA_PATH + "Assets/";
	public static string F_ASSETS_PATH = Application.dataPath + "/";
	public const string P_ASSETS_PATH = ASSETS + "/";
	public const string P_RESOURCES_PATH = P_ASSETS_PATH + RESOURCES + "/";
	public const string UGUI_ROOT = "UGUIRoot";
	public const string VERSION = "Version";
	public const string FILE_LIST = "FileList";
	public const string DYNAMIC_SECRET_FILE = "DynamicSecretKey.bytes";
	public const string HOTFIX = "HotFix";                                              // 主要的热更程序集名字
																						// 语言名
	public const string LANGUAGE_CHINESE_TRADITIONAL = "ChineseTraditional";            // 中文繁体语言的名字
	public const string LANGUAGE_CHINESE = "Chinese";                                   // 中文简体语言的名字
	public const string LANGUAGE_ENGLISH = "English";                                   // 英文语言的名字

	public const string RUNTIME_SETTINGS_RES_PATH = "Settings/FrameSettings.asset";     // Resources 下的运行时设置路径
}