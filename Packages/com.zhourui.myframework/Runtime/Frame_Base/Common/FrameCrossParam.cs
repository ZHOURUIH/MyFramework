using System.Collections.Generic;

// 用于存储跨域数据,在非热更时存储,热更时读取
public class FrameCrossParam
{
	public static string mLocalizationName;						// 当前选择的语言类型
	public static string mDownloadURL;							// 下载地址
	public static string mStreamingAssetsVersion;				// StreamingAssets中的版本号
	public static string mPersistentDataVersion;				// PersistentData中的版本号
	public static string mRemoteVersion;						// 远端的版本号
	public static Dictionary<string, GameFileInfo> mStreamingAssetsFileList = new();
	public static Dictionary<string, GameFileInfo> mPersistentAssetsFileList = new();
	public static Dictionary<string, GameFileInfo> mRemoteAssetsFileList = new();
	public static List<string> mTotalDownloadedFiles = new();   // 已经下载的文件列表,用于统计下载文件记录
	public static long mTotalDownloadByteCount;                 // 已经消耗的总下载量,单位字节,用于统计下载字节数
	public static ASSET_READ_PATH mAssetReadPath;				// 资源路径类型
}