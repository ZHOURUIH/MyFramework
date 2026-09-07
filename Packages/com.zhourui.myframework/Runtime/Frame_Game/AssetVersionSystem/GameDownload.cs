using System;
using System.Collections.Generic;
using static StringUtility;
using static FileUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static FrameBase;

// 用于更新资源
public class GameDownload
{
	protected List<string> mNeedDownloadFileList = new();           // 需要下载的资源
	protected List<string> mDynamicDownloadList = new();			// 只动态下载的资源,此目录中的资源不会在更新时下载,只是在需要加载时才会下载
	protected GameDownloadCallback mProgressCallback;				// 下载进度的回调
	protected GameDownloadTipCallback mTipCallback;					// 下载提示的回调
	protected DateTime mDownloadingTimer;							// 用于计时更新下载速度
	protected string mDownloadWritePath = F_PERSISTENT_ASSETS_PATH; // 默认下载到PersistentPath中
	protected int mDownloadedCount;									// 已经下载的文件数量
	protected int mDownloadSpeed;                                   // 下载速度
	protected int mDownloadGeneration;                              // 下载流程代数,玩家重试后让上一轮遗留回调失效
	protected int mAutoRetryCount = 3;								// 单个文件允许的自动重试次数
	protected int mRemainRetryCount = 3;							// 当前文件剩余自动重试次数,没有剩余次数时才会提示玩家是否重试
	protected bool mAllFinish = true;								// 是否已经全部完成
	protected bool mNeedWritePersistentFileList;					// 是否在完成时写入Persist的文件列表
	public GameDownload()
	{
		// 设置动态下载的列表
		mDynamicDownloadList.AddRange(FrameSettings.getDynamicDownloadList());
	}
	public void willDestroy()
	{
		// 如果在未更新完成就关闭了程序,则尽量先提交当前FileList,最后再写版本号。
		// VERSION作为提交标记必须最后写,避免出现版本号已经更新但FileList仍然是旧状态。
		if (!mAllFinish)
		{
			if (updateLocalFileList(false))
			{
				string localVersion = mAssetVersionSystem.getLocalVersion();
				if (!localVersion.isEmpty())
				{
					writeTxtFileSafe(mDownloadWritePath + VERSION, localVersion);
				}
			}
			mAllFinish = true;
		}
	}
	public void setErrorCallback(GameDownloadTipCallback callback) { mTipCallback = callback; }
	public void setProgressCallback(GameDownloadCallback callback) { mProgressCallback = callback; }
	public void setAutoRetryCount(int count)
	{
		mAutoRetryCount = Math.Max(0, count);
		mRemainRetryCount = mAutoRetryCount;
	}
	public void start()
	{
		// 每次start都是一轮独立流程,旧请求即使晚到也不能再修改当前下载状态。
		int generation = ++mDownloadGeneration;
		// start既可能是首次启动,也可能是玩家点击“重试”后重新开始。
		// 每次都必须重新建立任务列表和索引,不能沿用上一次失败流程的状态。
		mNeedDownloadFileList.Clear();
		mDownloadedCount = 0;
		mDownloadSpeed = 0;
		mRemainRetryCount = mAutoRetryCount;
		// 未启用热更时可以不进行下载
		if (isEditor() || !isEnableHotFix())
		{
			allFinished();
		}
		else
		{
			startCheckVersion(generation);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void startCheckVersion(int generation)
	{
		if (generation != mDownloadGeneration)
		{
			return;
		}
		logBase("下载目录:" + mDownloadWritePath);
		logBase("资源下载地址:" + mResourceManager.getDownloadURL());
		mAllFinish = false;

		mProgressCallback?.Invoke(0.0f, PROGRESS_TYPE.CHECKING_UPDATE, "", 0, 0);

		// 检查是否需要更新安装包,移动端会判断是否需要重新下载整个安装包
		string remoteVersion = mAssetVersionSystem.getRemoteVersion();
		string localVersion = mAssetVersionSystem.getLocalVersion();
		VERSION_COMPARE fullCompare = compareVersion3(remoteVersion, localVersion, out _, out VERSION_COMPARE bigCompare);
		// 如果本地版本号大于远端的,则不下载,此时远端资源还未上传,本地可以直接正常运行
		// 仅限安装的是全量资源包,才能从StreamingAssets中读取,如果不是全量资源包,则无法运行,但是此处无法判断是否为全量,只能默认为全量
		if (fullCompare == VERSION_COMPARE.REMOTE_LOWER)
		{
			// 根据StreamingAssets的文件数来判断是否为全量包,为了保险起见,文件数量小于等于5个时为非全量包。
			// 非全量包本身无法独立运行,远端版本又低于本地时也无法通过远端补齐资源,必须真正阻止启动。
			if (mAssetVersionSystem.getStreamingAssetsFile().Count <= 5)
			{
				logErrorBase("当前不是全量安装包,且本地版本号大于远端版本号,无法运行游戏");
				mTipCallback?.Invoke(DOWNLOAD_ERROR.LOCAL_VERSION_HIGHER);
				return;
			}
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
			mTipCallback?.Invoke(DOWNLOAD_ERROR.NONE);
			allFinished();
			return;
		}
		// 大版本号低于远端,本地无法直接升级大版本号,所以直接忽略,如果网络消息版本号匹配就继续运行,不匹配就会提示退出
		if (bigCompare == VERSION_COMPARE.LOCAL_LOWER)
		{
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.PERSISTENT_FIRST);
			mTipCallback?.Invoke(DOWNLOAD_ERROR.NONE);
			allFinished();
			return;
		}

		var streamingFiles = mAssetVersionSystem.getStreamingAssetsFile();
		var persistentFiles = mAssetVersionSystem.getPersistentAssetsFile();
		var remoteFiles = mAssetVersionSystem.getRemoteAssetsFile();
		logBase("本地StreamingAssets文件数量：" + streamingFiles.Count);
		logBase("本地PersistentAssets文件数量：" + persistentFiles.Count);
		logBase("远端文件数量：" + remoteFiles.Count);

		mProgressCallback?.Invoke(0.0f, PROGRESS_TYPE.DELETE_FILE, "", 0, 0);

		// 删除文件,只能删除Persistent中的文件,但是列表中的元素还是需要都删除掉
		// Persistent中需要删除列表记录,删除文件
		DateTime start = DateTime.Now;
		List<string> deleteFileList = checkDeleteFile(remoteFiles, persistentFiles);
		logBase("需要删除" + deleteFileList.Count + "个文件");
		foreach (string fileToDelete in deleteFileList)
		{
			persistentFiles.Remove(fileToDelete);
			string fullPath = F_PERSISTENT_ASSETS_PATH + fileToDelete;
			logBase("删除文件:" + fullPath);
			if (!deleteFile(fullPath))
			{
				logErrorBase("删除文件失败:" + fullPath);
			}
			mNeedWritePersistentFileList = true;
		}

		// StreamingAssets中无法删除文件,只能删除列表记录
		foreach (string fileToDelete in checkDeleteFile(remoteFiles, streamingFiles))
		{
			streamingFiles.Remove(fileToDelete);
		}

		logBase("删除文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");

		// 要下载的文件,其中不包含版本文件,下载新文件,每次只下载一个文件
		DateTime start1 = DateTime.Now;
		checkNeedDownloadFile(mNeedDownloadFileList, streamingFiles, persistentFiles, remoteFiles, mDynamicDownloadList);
		logBase("对比需要下载的文件列表耗时:" + (int)(DateTime.Now - start1).TotalMilliseconds + "毫秒");
		mNeedDownloadFileList.Remove(VERSION);
		logBase("需要下载" + mNeedDownloadFileList.Count + "个文件");
		mTipCallback?.Invoke(DOWNLOAD_ERROR.NONE);
		if (mNeedDownloadFileList.Count == 0)
		{
			allFinished();
		}
		else
		{
			mDownloadingTimer = DateTime.Now;
			downloadFile(mDownloadedCount, generation);
		}
	}
	// 下载普通资源文件
	protected void downloadFile(int index, int generation)
	{
		if (generation != mDownloadGeneration)
		{
			return;
		}
		if (index < 0 || index >= mNeedDownloadFileList.Count)
		{
			logErrorBase("下载文件索引越界,index:" + index + ", count:" + mNeedDownloadFileList.Count);
			return;
		}
		string fileName = mNeedDownloadFileList[index];
		downloadProgress(fileName, index, 0.0f);
		ResourceUtility.loadAssetsFromUrl(mResourceManager.getDownloadURL() + fileName, (byte[] bytes) =>
		{
			if (generation != mDownloadGeneration)
			{
				return;
			}
			// 单个资源文件下载完毕
			if (bytes == null)
			{
				logWarningBase("下载失败! " + fileName);
				// 单个文件失败时只重试当前文件,不要重新进入startCheckVersion。
				// 否则旧下载回调和新流程可能同时推进,并且任务列表/索引会互相污染。
				if (mRemainRetryCount > 0)
				{
					--mRemainRetryCount;
					downloadFile(index, generation);
				}
				else
				{
					mTipCallback?.Invoke(DOWNLOAD_ERROR.DOWNLOAD_FAILED);
				}
				return;
			}
			mAssetVersionSystem.addDownloadedInfo(bytes.Length, getFileNameWithSuffix(fileName));

			// 先检查下载内容,校验通过以后才能写正式文件和更新Persistent文件表。
			if (!mAssetVersionSystem.getRemoteAssetsFile().TryGetValue(fileName, out GameFileInfo remoteInfo))
			{
				logWarningBase("已下载的文件不存在与远端文件列表, 下载的文件:" + fileName);
				mTipCallback?.Invoke(DOWNLOAD_ERROR.NOT_IN_REMOTE_FILE_LIST);
				return;
			}

			GameFileInfo localInfo = new();
			localInfo.mFileName = fileName;
			localInfo.mFileSize = bytes.Length;
			localInfo.mMD5 = generateFileMD5(bytes);
			if (remoteInfo.mFileName != localInfo.mFileName ||
				remoteInfo.mFileSize != localInfo.mFileSize ||
				remoteInfo.mMD5 != localInfo.mMD5)
			{
				logWarningBase("下载的文件信息与远端的信息不一致:下载的信息:" + localInfo.mFileName + ", " + localInfo.mFileSize + ", " + localInfo.mMD5 +
						", 远端的信息:" + remoteInfo.mFileName + ", " + remoteInfo.mFileSize + ", " + remoteInfo.mMD5);
				if (mRemainRetryCount > 0)
				{
					--mRemainRetryCount;
					downloadFile(index, generation);
				}
				else
				{
					mTipCallback?.Invoke(DOWNLOAD_ERROR.VERIFY_FAILED);
				}
				// 校验失败后必须结束旧回调,绝对不能继续++mDownloadedCount。
				return;
			}

			// 校验通过以后先写临时文件,再替换正式文件。
			// 即使写盘中途杀进程,也不会把半个文件直接留在正式资源路径中。
			string finalPath = mDownloadWritePath + fileName;
			if (!writeFileSafe(finalPath, bytes, bytes.Length))
			{
				logWarningBase("写入下载文件失败:" + finalPath);
				if (mRemainRetryCount > 0)
				{
					--mRemainRetryCount;
					downloadFile(index, generation);
				}
				else
				{
					mTipCallback?.Invoke(DOWNLOAD_ERROR.WRITE_FAILED);
				}
				return;
			}
			mAssetVersionSystem.getPersistentAssetsFile().set(fileName, localInfo);
			mNeedWritePersistentFileList = true;
			// 每个文件都拥有完整的自动重试次数。
			mRemainRetryCount = mAutoRetryCount;

			// 所有文件已经下载完毕
			if (++mDownloadedCount >= mNeedDownloadFileList.Count)
			{
				allFinished();
			}
			// 还没下载完,下载下一个文件,这里延迟执行,避免可能的递归太深,导致栈溢出
			else
			{
				downloadFile(mDownloadedCount, generation);
			}
		}, (ulong downloaded, int downloadDelta, double deltaTimeMillis, float progress)=>
		{
			if (generation != mDownloadGeneration)
			{
				return;
			}
			mDownloadSpeed = (int)(downloadDelta * 1000 / (float)deltaTimeMillis);
			if ((DateTime.Now - mDownloadingTimer).TotalSeconds > 1.0f)
			{
				mDownloadingTimer = DateTime.Now;
				downloadProgress(fileName, index, progress);
			}
		});
	}
	// 计算剩余下载时间
	protected void downloadProgress(string fileName, int index, float progress)
	{
		var remoteFiles = mAssetVersionSystem.getRemoteAssetsFile();
		// 计算剩余的下载字节数,计算剩余时间
		int allCount = mNeedDownloadFileList.Count;
		ulong remainBytes = (ulong)(remoteFiles.get(mNeedDownloadFileList[index]).mFileSize * (1.0f - progress));
		for (int i = index + 1; i < allCount; ++i)
		{
			remainBytes += (ulong)remoteFiles.get(mNeedDownloadFileList[i]).mFileSize;
		}
		int remainTime = mDownloadSpeed != 0 ? (int)(remainBytes / (ulong)mDownloadSpeed) : 0;
		mProgressCallback?.Invoke(allCount != 0 ? index / (float)allCount : 0.0f, PROGRESS_TYPE.DOWNLOAD_RESOURCE, fileName, mDownloadSpeed, remainTime);
	}
	// 所有资源更新完毕
	protected void allFinished()
	{
		// FileList描述的是已经真正落盘的资源状态,必须先提交FileList。
		// VERSION最后写入,把它作为整次资源更新的提交标记。
		if (!updateLocalFileList(true))
		{
			return;
		}
		string remoteVersion = mAssetVersionSystem.getRemoteVersion();
		if (!remoteVersion.isEmpty())
		{
			if (!writeTxtFileSafe(mDownloadWritePath + VERSION, remoteVersion))
			{
				logErrorBase("写入资源版本号失败:" + mDownloadWritePath + VERSION);
				mTipCallback?.Invoke(DOWNLOAD_ERROR.WRITE_FAILED);
				return;
			}
			mAssetVersionSystem.setPersistentDataVersion(remoteVersion);
		}

		// 游戏更新完毕
		mAllFinish = true;
		mProgressCallback?.Invoke(1.0f, PROGRESS_TYPE.FINISH, "", 0, 0);
	}
	protected bool updateLocalFileList(bool notifyError)
	{
		if (!mNeedWritePersistentFileList)
		{
			return true;
		}
		if (!writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList()))
		{
			logErrorBase("写入本地FileList失败:" + F_PERSISTENT_ASSETS_PATH + FILE_LIST);
			if (notifyError)
			{
				mTipCallback?.Invoke(DOWNLOAD_ERROR.WRITE_FAILED);
			}
			return false;
		}
		mNeedWritePersistentFileList = false;
		logBase("本地文件信息列表更新完毕");
		return true;
	}
}