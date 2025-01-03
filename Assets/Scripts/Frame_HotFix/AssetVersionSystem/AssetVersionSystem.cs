using System;
using System.Collections.Generic;
using System.Text;
using static CSharpUtility;
using static UnityUtility;
using static FrameDefine;
using static FileUtility;
using static StringUtility;
using static BinaryUtility;
using static FrameUtility;
using static FrameBase;
using static FrameDefineBase;
using static FrameEditorUtility;

// 用于检测资源的版本号
public class AssetVersionSystem : FrameSystem
{
	protected Dictionary<string, GameFileInfo> mStreamingAssetsFileList = new();
	protected Dictionary<string, GameFileInfo> mPersistentAssetsFileList = new();
	protected Dictionary<string, GameFileInfo> mRemoteAssetsFileList = new();
	protected List<string> mTotalDownloadedFiles = new();			// 已经下载的文件列表,用于统计下载文件记录
	protected StringCallback mDownloadRemoteFileListCallback;
	protected string mStreamingAssetsVersion;
	protected string mPersistentAssetsVersion;
	protected string mRemoteAssetsVersion;
	protected Action mRemoteFileListFailCallback;
	protected long mTotalDownloadByteCount;                         // 已经消耗的总下载量,单位字节,用于统计下载字节数
	protected bool mPersistentDone;
	protected bool mStreamingDone;
	protected bool mRemoteDone;
	protected bool mCheckFileListFailed;
	protected static ASSET_READ_PATH mReadPathType = ASSET_READ_PATH.SAME_TO_REMOTE;
	public void addDownloadedInfo(int byteCount, string fileName)
	{
		mTotalDownloadByteCount += byteCount;
		mTotalDownloadedFiles.Add(fileName);
	}
	public long getDownloadedByteCount() { return mTotalDownloadByteCount; }
	public List<string> getDownloadedFiles() { return mTotalDownloadedFiles; }
	public void setDownloadedFiles(List<string> list) { mTotalDownloadedFiles.setRange(list); }
	public void clearDownloadedInfo()
	{
		mTotalDownloadByteCount = 0;
		mTotalDownloadedFiles.Clear();
	}
	// 未启用热更或者本地版本号大于远端版本号时,都应该设置为强制从StreamingAssets中加载
	public static void setReadPathType(ASSET_READ_PATH pathType) { mReadPathType = pathType; }
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
	public Dictionary<string, GameFileInfo> getStreamingAssetsFile() { return mStreamingAssetsFileList; }
	public Dictionary<string, GameFileInfo> getPersistentAssetsFile() { return mPersistentAssetsFileList; }
	public Dictionary<string, GameFileInfo> getRemoteAssetsFile() { return mRemoteAssetsFileList; }
	public void addPersistentFile(GameFileInfo info)
	{
		mPersistentAssetsFileList.TryAdd(info.mFileName, info);
	}
	public void syncStreamingAssetToPersistentFileInfo(string file)
	{
		if (!mStreamingAssetsFileList.TryGetValue(file, out GameFileInfo streamingAsset))
		{
			return;
		}
		if (mPersistentAssetsFileList.TryGetValue(file, out GameFileInfo info))
		{
			info.mFileSize = streamingAsset.mFileSize;
			info.mMD5 = streamingAsset.mMD5;
		}
		else
		{
			mPersistentAssetsFileList.Add(file, new()
			{
				mFileName = file,
				mFileSize = streamingAsset.mFileSize,
				mMD5 = streamingAsset.mMD5
			});
		}
	}
	public string generateStreamingAssetFileList()
	{
		StringBuilder fileString = new();
		fileString.Append(IToS(mStreamingAssetsFileList.Count));
		fileString.Append("\n");
		foreach (GameFileInfo item in mStreamingAssetsFileList.Values)
		{
			item.toString(fileString);
			fileString.Append("\n");
		}
		return fileString.ToString();
	}
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
	public void setStreamingAssetsFile(Dictionary<string, GameFileInfo> infoList)
	{
		mStreamingAssetsFileList.setRange(infoList.safe());
	}
	public void setPersistentAssetsFile(Dictionary<string, GameFileInfo> infoList)
	{
		mPersistentAssetsFileList.setRange(infoList.safe());
	}
	public void setRemoteAssetsFile(Dictionary<string, GameFileInfo> infoList)
	{
		mRemoteAssetsFileList.setRange(infoList.safe());
	}
	public void openStreamingAssetAndPersistentVersion(Action callback)
	{
		openTxtFileAsync(F_ASSET_BUNDLE_PATH + VERSION, !isEditor(), (string text0) =>
		{
			openTxtFileAsync(F_PERSISTENT_ASSETS_PATH + VERSION, false, (string text1) =>
			{
				setStreamingAssetsVersion(text0);
				setPersistentAssetsVersion(text1);
				callback();
			});
		});
	}
	public void startCheckFileList(bool enableHotFix, List<string> ignorePath, List<string> ignoreFile, Action successCallback, Action failCallback, CheckAndDownloadFileListCallback remoteFileListCallback)
	{
		log("Remote Version:" + getRemoteAssetsVersion() +
				", Local Version:" + getLocalVersion() +
				", streamingAssetsVersion:" + getStreamingAssetsVersion() +
				", persistVersion:" + getPersistentAssetsVersion() +
				", streamingVersionPath:" + F_ASSET_BUNDLE_PATH + VERSION +
				", persistentVersionPath:" + F_PERSISTENT_ASSETS_PATH + VERSION);

		mRemoteFileListFailCallback = failCallback;
		mPersistentDone = false;
		mStreamingDone = false;
		mRemoteDone = false;
		mCheckFileListFailed = false;

		// 全部的文件列表都检查完毕后就执行doneCallback,检查失败就取消等待
		mWaitingManager.createWaiting(
			()=> { return mPersistentDone && mStreamingDone && mRemoteDone; }, 
			successCallback).
			setCancelCondition(() => { return mCheckFileListFailed; });

		log("开始获取所有文件列表");
		// 获取StreamingAssets,PersistentPath的所有文件信息
		openFileList(F_ASSET_BUNDLE_PATH, () =>
		{
			log("获取StreamingAssets文件列表完成");
			mStreamingDone = true;
		}, ignorePath, ignoreFile);
		// 编辑器下和不下载更新的版本中不获取远端文件列表和PersistentPath的文件列表
		string remoteVersion = mAssetVersionSystem.getRemoteAssetsVersion();
		string localVersion = mAssetVersionSystem.getLocalVersion();
		VERSION_COMPARE fullCompare = compareVersion3(remoteVersion, localVersion, out _, out _);
		// 如果本地版本号大于远端的,则不下载,此时远端资源还未上传,本地可以直接正常运行,认为安装的是全量包
		if (isEditor() || !enableHotFix || fullCompare == VERSION_COMPARE.REMOTE_LOWER)
		{
			mPersistentDone = true;
			mRemoteDone = true;
			return;
		}

		openFileList(F_PERSISTENT_ASSETS_PATH, () =>
		{
			log("获取PersistentPath文件列表完成");
			mPersistentDone = true;
		}, ignorePath, ignoreFile);
		// 先读本地缓存的文件列表
		openFileAsync(F_PERSISTENT_ASSETS_PATH + FILE_LIST_REMOTE, false, (byte[] content) =>
		{
			remoteFileListCallback(content, (byte[] remoteContent) => { checkRemoteList(remoteContent); });
		});
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkRemoteList(byte[] contentBytes)
	{
		using var a = new DicScope<string, GameFileInfo>(out var remoteFileList);
		DateTime start0 = DateTime.Now;
		// 先检查本地的文件信息是否有效
		parseFileList(bytesToString(contentBytes), remoteFileList);
		if (remoteFileList.isEmpty())
		{
			mCheckFileListFailed = true;
			mRemoteFileListFailCallback?.Invoke();
			return;
		}
		log("获取远端文件列表耗时:" + (int)(DateTime.Now - start0).TotalMilliseconds + "毫秒");
		remoteFileList.Remove(RESOURCE_AVAILABLE_FILE);
		remoteFileList.Remove(VERSION);
		remoteFileList.Remove(FILE_LIST);
		remoteFileList.Remove(FILE_LIST_MD5);
		setRemoteAssetsFile(remoteFileList);
		log("远端资源文件数量:" + remoteFileList.Count);
		mRemoteDone = true;
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
				using var a = new DicScope<string, GameFileInfo>(out var fileInfoList);
				bool isSame = true;
				parseFileList(content, fileInfoList);
				List<string> fileList = null;
				// 扫描本地文件进行校验,只有PersistentPath中的才能扫描
				if (path == F_PERSISTENT_ASSETS_PATH)
				{
					fileList = findFilesNonAlloc(path);
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
						using var b = new ListScope<string>(out var existFileList);
						existFileList.addRange(fileInfoList.Keys).Sort();
						fileList.Sort();
						for (int i = 0; i < fileList.Count; ++i)
						{
							if (existFileList[i] != fileList[i])
							{
								log("因为文件列表中记录的文件与实际的不一致,所以重新扫描:" + fileListFullPath);
								isSame = false;
								break;
							}
						}
					}
					else
					{
						log("因为文件列表中文件数量与实际的不一致,所以重新扫描:" + fileListFullPath);
					}
				}
				if (isSame)
				{
					setFileListToAssetSystem(path, fileInfoList);
					log("读取文件列表耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒, file:" + fileListFullPath);
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
				bool isListFromPool = false;
				List<string> fileList = null;
				if (path == F_ASSET_BUNDLE_PATH)
				{
					isListFromPool = true;
					LIST(out fileList);
					findStreamingAssetsFiles(path, fileList, EMPTY, true, true);
				}
				else
				{
					fileList = findFilesNonAlloc(path);
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
					log("本地找不到文件列表,重新遍历本地文件生成," + path);
				}
				else
				{
					log("本地文件为空,path:" + path);
				}
				generateLocalFileList(path, fileList, callback);
				if (isListFromPool)
				{
					UN_LIST(ref fileList);
				}
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
				log("遍历本地文件生成文件列表耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒, path:" + path);
				callback?.Invoke();
			}
		});
	}
	protected void setFileListToAssetSystem(string path, Dictionary<string, GameFileInfo> fileInfoList)
	{
		if (path == F_ASSET_BUNDLE_PATH)
		{
			setStreamingAssetsFile(fileInfoList);
			log("StreamingAssets资源文件数量:" + fileInfoList.count());
		}
		else if (path == F_PERSISTENT_ASSETS_PATH)
		{
			setPersistentAssetsFile(fileInfoList);
			log("PersistentPath资源文件数量:" + fileInfoList.count());
		}
		else
		{
			logError("setFileListToAssetSystem path error:" + path);
		}
	}
}