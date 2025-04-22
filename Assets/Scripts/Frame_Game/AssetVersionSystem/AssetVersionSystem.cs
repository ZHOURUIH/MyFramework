using System;
using System.Collections.Generic;
using System.Text;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static FileUtility;
using static StringUtility;

// 用于管理资源的版本信息
public class AssetVersionSystem : FrameSystem
{
	protected Dictionary<string, GameFileInfo> mStreamingAssetsFileList = new();
	protected Dictionary<string, GameFileInfo> mPersistentAssetsFileList = new();
	protected Dictionary<string, GameFileInfo> mRemoteAssetsFileList = new();
	protected List<string> mTotalDownloadedFiles = new();           // 已经下载的文件列表,用于统计下载文件记录
	protected long mTotalDownloadByteCount;                         // 已经消耗的总下载量,单位字节,用于统计下载字节数
	protected Action mRemoteFileListFailCallback;
	protected Action mSuccessCallback;
	protected ASSET_READ_PATH mAssetReadPath;
	protected string mRemoteVersion;
	protected string mStreamingAssetsVersion;
	protected string mPersistentDataVersion;
	protected bool mPersistentDone;
	protected bool mStreamingDone;
	protected bool mRemoteDone;
	protected bool mCheckFileListFailed;
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mPersistentDone && mStreamingDone && mRemoteDone && !mCheckFileListFailed)
		{
			mSuccessCallback?.Invoke();
		}
	}
	public long getTotalDownloadedByteCount() { return mTotalDownloadByteCount; }
	public List<string> getTotalDownloadedFiles() { return mTotalDownloadedFiles; }
	public void addDownloadedInfo(int byteCount, string fileName)
	{
		mTotalDownloadByteCount += byteCount;
		mTotalDownloadedFiles.Add(fileName);
	}
	// 获取文件的加载路径,filePath是StreamingAssets下的相对路径
	public string getFileReadPath(string filePath)
	{
		if (mAssetReadPath == ASSET_READ_PATH.SAME_TO_REMOTE)
		{
			// 远端没有此文件
			if (!mRemoteAssetsFileList.TryGetValue(filePath, out GameFileInfo remoteInfo))
			{
				// 完全没有此文件信息,无法加载
				logErrorBase("远端没有此文件,filePath:" + filePath);
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
		else if (mAssetReadPath == ASSET_READ_PATH.PERSISTENT_FIRST)
		{
			// 优先从Persistent中读取
			if (mPersistentAssetsFileList.TryGetValue(filePath, out GameFileInfo persistentInfo))
			{
				return F_PERSISTENT_ASSETS_PATH + filePath;
			}
			// 没有再读取streamingAssets中的文件
			if (mStreamingAssetsFileList.TryGetValue(filePath, out GameFileInfo streamingInfo))
			{
				return F_ASSET_BUNDLE_PATH + filePath;
			}
			// 本地没有此文件,则从远端下载
			return null;
		}
		else if (mAssetReadPath == ASSET_READ_PATH.STREAMING_ASSETS_ONLY)
		{
			return F_ASSET_BUNDLE_PATH + filePath;
		}
		else if (mAssetReadPath == ASSET_READ_PATH.REMOTE_ASSETS_ONLY)
		{
			// 返回null,会自动开始下载
			return null;
		}
		logErrorBase("无法获取文件路径,filePath:" + filePath);
		return null;
	}
	public void setRemoteVersion(string version) { mRemoteVersion = version ?? "0.0.0"; }
	public void setStreamingAssetsVersion(string version) { mStreamingAssetsVersion = version ?? "0.0.0"; }
	public void setPersistentDataVersion(string version) { mPersistentDataVersion = version ?? "0.0.0"; }
	public void setAssetReadPath(ASSET_READ_PATH read) { mAssetReadPath = read; }
	public string getRemoteVersion() { return mRemoteVersion; }
	public string getStreamingAssetsVersion() { return mStreamingAssetsVersion; }
	public string getPersistentDataVersion() { return mPersistentDataVersion; }
	public ASSET_READ_PATH getAssetReadPath() { return mAssetReadPath; }
	public Dictionary<string, GameFileInfo> getStreamingAssetsFile() { return mStreamingAssetsFileList; }
	public Dictionary<string, GameFileInfo> getPersistentAssetsFile() { return mPersistentAssetsFileList; }
	public Dictionary<string, GameFileInfo> getRemoteAssetsFile() { return mRemoteAssetsFileList; }
	public string getLocalVersion()
	{
		if (mStreamingAssetsVersion == null && mPersistentDataVersion == null)
		{
			return null;
		}
		// 选择更高版本号的
		if (mPersistentDataVersion == null ||
			compareVersion3(mStreamingAssetsVersion, mPersistentDataVersion, out _, out _) == VERSION_COMPARE.LOCAL_LOWER)
		{
			return mStreamingAssetsVersion;
		}
		return mPersistentDataVersion;
	}
	// remoteFileListCallback是获取到最新的远端文件列表
	public void startCheckFileList(bool enableHotFix, string remoteFileListMD5, List<string> ignorePath, List<string> ignoreFile, Action successCallback, Action failCallback, DownloadFileListCallback remoteFileListCallback)
	{
		logBase("Remote Version:" + mRemoteVersion +
				", Local Version:" + getLocalVersion() +
				", streamingAssetsVersion:" + mStreamingAssetsVersion +
				", persistVersion:" + mPersistentDataVersion +
				", streamingVersionPath:" + F_ASSET_BUNDLE_PATH + VERSION +
				", persistentVersionPath:" + F_PERSISTENT_ASSETS_PATH + VERSION);

		mRemoteFileListFailCallback = failCallback;
		mPersistentDone = false;
		mStreamingDone = false;
		mRemoteDone = false;
		mCheckFileListFailed = false;
		ignoreFile ??= new();
		ignoreFile.addUnique(VERSION);
		ignoreFile.addUnique(FILE_LIST);
		ignoreFile.addUnique(FILE_LIST_MD5);
		ignoreFile.addUnique(FILE_LIST_REMOTE);
		ignorePath ??= new();
		ignorePath.addUnique("/temp/");
		mSuccessCallback = successCallback;

		logBase("开始获取所有文件列表");
		// 获取StreamingAssets,PersistentPath的所有文件信息
		openFileList(F_ASSET_BUNDLE_PATH, () =>
		{
			logBase("获取StreamingAssets文件列表完成");
			mStreamingDone = true;
		}, ignorePath, ignoreFile);
		// 编辑器下和不下载更新的版本中不获取远端文件列表和PersistentPath的文件列表
		// 如果本地版本号大于远端的,则不下载,此时远端资源还未上传,本地可以直接正常运行,认为安装的是全量包
		if (isEditor() || 
			!enableHotFix || 
			compareVersion3(mRemoteVersion, getLocalVersion(), out _, out _) == VERSION_COMPARE.REMOTE_LOWER)
		{
			mPersistentDone = true;
			mRemoteDone = true;
			return;
		}

		openFileList(F_PERSISTENT_ASSETS_PATH, () =>
		{
			logBase("获取PersistentPath文件列表完成");
			mPersistentDone = true;
		}, ignorePath, ignoreFile);

		// 先读本地缓存的文件列表
		// 如果本地的信息与远端的不一致,再下载,因为要减少不必要的下载量
		// 减少流量消耗以及时间消耗,下载一次可能会需要消耗500毫秒
		openFileAsync(F_PERSISTENT_ASSETS_PATH + FILE_LIST_REMOTE, false, (byte[] content) =>
		{
			string localMD5 = generateFileMD5(content);
			if (content == null || remoteFileListMD5 != localMD5)
			{
				remoteFileListCallback((byte[] content0) =>
				{
					if (content0 == null)
					{
						mRemoteFileListFailCallback?.Invoke();
						return;
					}
					// 写到本地
					writeFile(F_PERSISTENT_ASSETS_PATH + FILE_LIST_REMOTE, content0, content0.Length);
					checkRemoteList(content0);
				});
			}
			else
			{
				checkRemoteList(content);
			}
		});
	}
	// path为绝对路径
	protected void openFileList(string path, Action callback, List<string> ignorePath, List<string> ignoreFile)
	{
		DateTime start = DateTime.Now;
		string fileListFullPath = path + FILE_LIST;
		// 本地已经有生成好的FileList文件,不过即使读取的是已经生成好的文件信息,也要再获取所有文件的文件名和大小进行校验,避免记录错误的信息
		openTxtFileAsync(fileListFullPath, false, (string content) =>
		{
			if (!content.isEmpty())
			{
				Dictionary<string, GameFileInfo> fileInfoList = new();
				bool isSame = true;
				parseFileList(content, fileInfoList);
				List<string> fileList = null;
				// 扫描本地文件进行校验,只有PersistentPath中的才能扫描
				if (path == F_PERSISTENT_ASSETS_PATH)
				{
					fileList = new();
					findFilesInternal(path, fileList, null, true);
					for (int i = 0; i < fileList.Count; ++i)
					{
						string file = fileList[i];
						if (isIgnorePath(file, ignorePath) || ignoreFile.Contains(getFileNameWithSuffix(file)))
						{
							fileList.RemoveAt(i--);
						}
						else
						{
							fileList[i] = file.removeStartCount(path.Length);
						}
					}
					isSame = fileInfoList.Count == fileList.Count;
					if (isSame)
					{
						List<string> existFileList = new(fileInfoList.Keys);
						existFileList.Sort();
						fileList.Sort();
						for (int i = 0; i < fileList.Count; ++i)
						{
							if (existFileList[i] != fileList[i])
							{
								logBase("因为文件列表中记录的文件与实际的不一致,所以重新扫描:" + fileListFullPath);
								isSame = false;
								break;
							}
						}
					}
					else
					{
						logBase("因为文件列表中文件数量与实际的不一致,所以重新扫描:" + fileListFullPath);
					}
				}
				if (isSame)
				{
					setFileListToAssetSystem(path, fileInfoList);
					logBase("读取文件列表耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒, file:" + fileListFullPath);
					callback?.Invoke();
				}
				else if (fileList != null)
				{
					for (int i = 0; i < fileList.Count; ++i)
					{
						fileList[i] = path + fileList[i];
					}
					generateLocalFileList(path, fileList, callback);
				}
			}
			// 本地没有FileList,则查找所有文件,生成文件信息列表
			else
			{
				List<string> fileList = new();
				if (path == F_ASSET_BUNDLE_PATH)
				{
					findStreamingAssetsFiles(path, fileList, null, true, true);
				}
				else
				{
					findFilesInternal(path, fileList, null, true);
				}
				if (!fileList.isEmpty())
				{
					int count = fileList.Count;
					for (int i = 0; i < fileList.Count; ++i)
					{
						if (isIgnorePath(fileList[i], ignorePath) || ignoreFile.Contains(getFileNameWithSuffix(fileList[i])))
						{
							fileList.RemoveAt(i--);
						}
					}
					logBase("本地找不到文件列表,重新遍历本地文件生成," + path);
				}
				else
				{
					logBase("本地文件为空,path:" + path);
				}
				generateLocalFileList(path, fileList, callback);
			}
		});
	}
	protected void generateLocalFileList(string path, List<string> fileList, Action callback)
	{
		if (fileList.isEmpty())
		{
			setFileListToAssetSystem(path, null);
			// PersistentPath中的FileList需要更新写入文件
			if (path == F_PERSISTENT_ASSETS_PATH)
			{
				writeFileList(path, generatePersistentAssetFileList());
			}
			callback?.Invoke();
			return;
		}

		DateTime start = DateTime.Now;
		Dictionary<string, GameFileInfo> fileInfoList = new();
		// 打开所有文件
		openFileListAsync(fileList, true, (string fileName, byte[] bytes) =>
		{
			if (bytes == null)
			{
				return;
			}
			string relativeFileName = fileName.removeStartCount(path.Length);
			GameFileInfo info = new()
			{
				mFileName = relativeFileName,
				mFileSize = bytes.Length,
				mMD5 = generateFileMD5(bytes)
			};
			fileInfoList.Add(relativeFileName, info);
			if (fileInfoList.Count == fileList.Count)
			{
				setFileListToAssetSystem(path, fileInfoList);
				// PersistentPath中的FileList需要更新写入文件
				if (path == F_PERSISTENT_ASSETS_PATH)
				{
					writeFileList(path, generatePersistentAssetFileList());
				}
				logBase("遍历本地文件生成文件列表耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒, path:" + path);
				callback?.Invoke();
			}
		});
	}
	protected void checkRemoteList(byte[] contentBytes)
	{
		Dictionary<string, GameFileInfo> remoteFileList = new();
		DateTime start0 = DateTime.Now;
		// 先检查本地的文件信息是否有效
		parseFileList(bytesToString(contentBytes), remoteFileList);
		if (remoteFileList.isEmpty())
		{
			mCheckFileListFailed = true;
			mRemoteFileListFailCallback?.Invoke();
			return;
		}
		logBase("获取远端文件列表耗时:" + (int)(DateTime.Now - start0).TotalMilliseconds + "毫秒");
		remoteFileList.Remove(VERSION);
		remoteFileList.Remove(FILE_LIST);
		remoteFileList.Remove(FILE_LIST_MD5);
		setRemoteAssetsFile(remoteFileList);
		logBase("远端资源文件数量:" + remoteFileList.Count);
		mRemoteDone = true;
	}
	protected void setFileListToAssetSystem(string path, Dictionary<string, GameFileInfo> fileInfoList)
	{
		if (path == F_ASSET_BUNDLE_PATH)
		{
			setStreamingAssetsFile(fileInfoList);
			logBase("StreamingAssets资源文件数量:" + fileInfoList.count());
		}
		else if (path == F_PERSISTENT_ASSETS_PATH)
		{
			setPersistentAssetsFile(fileInfoList);
			logBase("PersistentPath资源文件数量:" + fileInfoList.count());
		}
		else
		{
			logErrorBase("setFileListToAssetSystem path error:" + path);
		}
	}
	protected bool isIgnorePath(string fullPath, List<string> ignorePath)
	{
		if (ignorePath == null)
		{
			return false;
		}
		foreach (string path in ignorePath)
		{
			if (fullPath.Contains(path))
			{
				return true;
			}
		}
		return false;
	}
	public string generatePersistentAssetFileList()
	{
		StringBuilder fileString = new();
		fileString.Append(mPersistentAssetsFileList.Count);
		fileString.Append("\n");
		foreach (GameFileInfo item in mPersistentAssetsFileList.Values)
		{
			item.toString(fileString);
			fileString.Append("\n");
		}
		return fileString.ToString();
	}
	protected void setStreamingAssetsFile(Dictionary<string, GameFileInfo> infoList) { mStreamingAssetsFileList.setRange(infoList); }
	protected void setPersistentAssetsFile(Dictionary<string, GameFileInfo> infoList) { mPersistentAssetsFileList.setRange(infoList); }
	protected void setRemoteAssetsFile(Dictionary<string, GameFileInfo> infoList) { mRemoteAssetsFileList.setRange(infoList); }
}