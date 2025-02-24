using UnityEngine;

public class FrameDefineBase
{
	public const string ASSETS = "Assets";
	public const string ANDROID = "Android";
	public const string WINDOWS = "Windows";
	public const string WEBGL = "WebGL";
	public const string IOS = "iOS";
	public const string MACOS = "MacOS";
	public const string P_ASSETS_PATH = ASSETS + "/";
	// 安卓真机上没有STREAMING_ASSETS的定义
#if UNITY_EDITOR
	public const string STREAMING_ASSETS = "StreamingAssets";
#elif UNITY_IOS
	public const string STREAMING_ASSETS = "Raw";
#elif !UNITY_ANDROID
	public const string STREAMING_ASSETS = "StreamingAssets";
#endif
#if UNITY_EDITOR
	public const string P_STREAMING_ASSETS_PATH = P_ASSETS_PATH + STREAMING_ASSETS + "/";
	public const string P_ASSET_BUNDLE_ANDROID_PATH = P_STREAMING_ASSETS_PATH + ANDROID + "/";
	public const string P_ASSET_BUNDLE_WINDOWS_PATH = P_STREAMING_ASSETS_PATH + WINDOWS + "/";
	public const string P_ASSET_BUNDLE_IOS_PATH = P_STREAMING_ASSETS_PATH + IOS + "/";
	public const string P_ASSET_BUNDLE_MACOS_PATH = P_STREAMING_ASSETS_PATH + MACOS + "/";
	public const string P_ASSET_BUNDLE_WEBGL_PATH = P_STREAMING_ASSETS_PATH + WEBGL + "/";
#if UNITY_ANDROID
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_ANDROID_PATH;
#elif UNITY_STANDALONE_WIN
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_WINDOWS_PATH;
#elif UNITY_IOS
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_IOS_PATH;
#elif UNITY_STANDALONE_OSX
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_MACOS_PATH;
#elif UNITY_WEBGL
	public const string P_ASSET_BUNDLE_PATH = P_ASSET_BUNDLE_WEBGL_PATH;
#endif
#endif
	// Windows下才能使用临时目录
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	public static string F_TEMPORARY_CACHE_PATH = Application.temporaryCachePath + "/";
#endif
	public static string F_STREAMING_ASSETS_PATH = Application.streamingAssetsPath + "/";
	// 各个平台下的AssetBundle路径不一样,为了避免打包资源时冲突
	public static string F_ASSET_BUNDLE_ANDROID_PATH = F_STREAMING_ASSETS_PATH + ANDROID + "/";
	public static string F_ASSET_BUNDLE_WINDOWS_PATH = F_STREAMING_ASSETS_PATH + WINDOWS + "/";
	public static string F_ASSET_BUNDLE_IOS_PATH = F_STREAMING_ASSETS_PATH + IOS + "/";
	public static string F_ASSET_BUNDLE_MACOS_PATH = F_STREAMING_ASSETS_PATH + MACOS + "/";
	public static string F_ASSET_BUNDLE_WEBGL_PATH = F_STREAMING_ASSETS_PATH + WEBGL + "/";
#if UNITY_ANDROID
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_ANDROID_PATH;
#elif UNITY_STANDALONE_WIN
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_WINDOWS_PATH;
#elif UNITY_IOS
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_IOS_PATH;
#elif UNITY_STANDALONE_OSX
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_MACOS_PATH;
#elif UNITY_WEBGL
	public static string F_ASSET_BUNDLE_PATH = F_ASSET_BUNDLE_WEBGL_PATH;
#endif
	public const string FILE_LIST = "FileList";
	public const string VERSION = "Version";
	public const string FILE_LIST_REMOTE = "FileList_Remote";
	public const string FILE_LIST_MD5 = "FileList_MD5";
	public const string DATA_SUFFIX = ".bytes";
	public const string HOTFIX = "HotFix";
	public const string HOTFIX_FRAME = "Frame_HotFix";
	public const string HOTFIX_FILE = HOTFIX + ".dll";
	public const string HOTFIX_FRAME_FILE = HOTFIX_FRAME + ".dll";
	public const string HOTFIX_BYTES_FILE = HOTFIX_FILE + DATA_SUFFIX;
	public const string HOTFIX_FRAME_BYTES_FILE = HOTFIX_FRAME_FILE + DATA_SUFFIX;
	// 以下是可扩展的参数,可以修改为自己项目需要的参数
	// UI的制作标准,所有UI都是按1280*960标准分辨率制作的
	public static int STANDARD_WIDTH = 1280;
	public static int STANDARD_HEIGHT = 960;
}