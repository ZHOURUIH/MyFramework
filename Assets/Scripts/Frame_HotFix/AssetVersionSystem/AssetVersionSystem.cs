using System.Collections.Generic;
using System.Text;
using static UnityUtility;
using static StringUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 用于检测资源的版本号
public class AssetVersionSystem : FrameSystem
{
	protected Dictionary<string, GameFileInfo> mStreamingAssetsFileList = new();    // StreamingAssets的资源信息列表
	protected Dictionary<string, GameFileInfo> mPersistentAssetsFileList = new();   // PersistentData的资源信息列表
	protected Dictionary<string, GameFileInfo> mRemoteAssetsFileList = new();		// 远端的资源信息列表
	protected List<string> mTotalDownloadedFiles = new();							// 已经下载的文件列表,用于统计下载文件记录
	protected string mStreamingAssetsVersion;										// StreamingAssets中的版本号
	protected string mPersistentAssetsVersion;                                      // PersistentData中的版本号
	protected string mRemoteAssetsVersion;											// 远端版本号
	protected long mTotalDownloadByteCount;											// 已经消耗的总下载量,单位字节,用于统计下载字节数
	protected static ASSET_READ_PATH mReadPathType;									// 资源路径的计算方式
	public AssetVersionSystem()
	{
		mReadPathType = ASSET_READ_PATH.SAME_TO_REMOTE;
	}
	public long getTotalDownloadedByteCount() { return mTotalDownloadByteCount; }
	public List<string> getTotalDownloadedFiles() { return mTotalDownloadedFiles; }
	public void setTotalDownloadedFiles(List<string> files) { mTotalDownloadedFiles.setRange(files); }
	public void setTotalDownloadedByteCount(long count) { mTotalDownloadByteCount = count; }
	public void clearDownloadedInfo()
	{
		mTotalDownloadByteCount = 0;
		mTotalDownloadedFiles.Clear();
	}
	// 未启用热更或者本地版本号大于远端版本号时,都应该设置为强制从StreamingAssets中加载
	public void setAssetReadPath(ASSET_READ_PATH pathType) { mReadPathType = pathType; }
	// 获取文件的加载路径,filePath是StreamingAssets下的相对路径
	public string getFileReadPath(string filePath)
	{
		if (mReadPathType == ASSET_READ_PATH.SAME_TO_REMOTE)
		{
			// 远端没有此文件
			if (!mRemoteAssetsFileList.TryGetValue(filePath, out GameFileInfo remoteInfo))
			{
				// 完全没有此文件信息,无法加载
				logError("远端没有此文件,filePath:" + filePath);
				return null;
			}
			// persistent中的文件信息与远端一致,则读取persistent中的文件
			if (mPersistentAssetsFileList.TryGetValue(filePath, out GameFileInfo persistentInfo) &&
				persistentInfo.mFileSize == remoteInfo.mFileSize &&
				persistentInfo.mMD5 == remoteInfo.mMD5)
			{
				return F_PERSISTENT_ASSETS_PATH + filePath;
			}
			// streamingAssets中的文件信息与远端一致,则读取streamingAssets中的文件
			if (mStreamingAssetsFileList.TryGetValue(filePath, out GameFileInfo streamingInfo) &&
				streamingInfo.mFileSize == remoteInfo.mFileSize &&
				streamingInfo.mMD5 == remoteInfo.mMD5)
			{
				return F_ASSET_BUNDLE_PATH + filePath;
			}
			// 本地没有此文件,则从远端下载
			return null;
		}
		else if (mReadPathType == ASSET_READ_PATH.STREAMING_ASSETS_ONLY)
		{
			return F_ASSET_BUNDLE_PATH + filePath;
		}
		else if (mReadPathType == ASSET_READ_PATH.REMOTE_ASSETS_ONLY)
		{
			// 返回null,会自动开始下载
			return null;
		}
		logError("无法获取文件路径,filePath:" + filePath);
		return null;
	}
	public void setStreamingAssetsVersion(string streamingVersion) { mStreamingAssetsVersion = streamingVersion; }
	public void setPersistentAssetsVersion(string persistentVersion) { mPersistentAssetsVersion = persistentVersion; }
	public void setRemoteVersion(string version) { mRemoteAssetsVersion = version; }
	public string getStreamingAssetsVersion() { return mStreamingAssetsVersion; }
	public string getPersistentAssetsVersion() { return mPersistentAssetsVersion; }
	public string getRemoteAssetsVersion() { return mRemoteAssetsVersion; }
	public string getLocalVersion()
	{
		if (mStreamingAssetsVersion == null && mPersistentAssetsVersion == null)
		{
			return null;
		}
		// 选择更高版本号的
		if (mPersistentAssetsVersion == null ||
			compareVersion3(mStreamingAssetsVersion, mPersistentAssetsVersion, out _, out _) == VERSION_COMPARE.LOCAL_LOWER)
		{
			return mStreamingAssetsVersion;
		}
		return mPersistentAssetsVersion;
	}
	public void addPersistentFile(GameFileInfo info) { mPersistentAssetsFileList.TryAdd(info.mFileName, info); }
	public string generatePersistentAssetFileList()
	{
		StringBuilder fileString = new();
		fileString.Append(IToS(mPersistentAssetsFileList.Count));
		fileString.Append("\n");
		foreach (GameFileInfo item in mPersistentAssetsFileList.Values)
		{
			item.toString(fileString);
			fileString.Append("\n");
		}
		return fileString.ToString();
	}
	public void setStreamingAssetsFile(Dictionary<string, GameFileInfo> infoList) { mStreamingAssetsFileList.setRange(infoList); }
	public void setPersistentAssetsFile(Dictionary<string, GameFileInfo> infoList) { mPersistentAssetsFileList.setRange(infoList); }
	public void setRemoteAssetsFile(Dictionary<string, GameFileInfo> infoList) { mRemoteAssetsFileList.setRange(infoList); }
}