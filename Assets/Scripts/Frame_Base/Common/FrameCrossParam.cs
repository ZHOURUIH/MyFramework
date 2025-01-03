using System.Collections.Generic;

// 用于存储跨域数据,在非热更时存储,热更时读取
public class FrameCrossParam
{
	public static string mPersistentDataVersion;										// 在PersistDataPath中的版本号
	public static string mStreamingAssetsVersion;										// 在StreamingAssets中的版本号
	public static string mRemoteVersion;												// 远端版本号
	public static string mDownloadURL;                                                  // 资源下载地址
	public static string mLocalizationName;												// 当前选择的语言类型
	public static FramworkParam mFramworkParam;											// 框架参数
	public static Dictionary<string, GameFileInfo> mStreamingAssetsFileList = new();    // StreamingAssets中的文件信息列表
	public static Dictionary<string, GameFileInfo> mPersistentAssetsFileList = new();   // PersistDataPath中的文件信息列表
	public static Dictionary<string, GameFileInfo> mRemoteAssetsFileList = new();       // 远端的文件信息列表
	public static List<string> mTotalDownloadedFiles = new();							// 已经下载的文件列表,用于统计下载文件记录
	public static ASSET_READ_PATH mReadPathType = ASSET_READ_PATH.SAME_TO_REMOTE;		// 资源读取路径选择
}