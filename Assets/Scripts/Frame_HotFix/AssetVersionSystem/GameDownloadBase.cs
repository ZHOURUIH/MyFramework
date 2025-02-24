using System;
using System.Collections.Generic;
using static FrameUtility;
using static StringUtility;
using static FileUtility;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameDefineBase;
using static MathUtility;
using static CSharpUtility;
using static FrameEditorUtility;

public class GameDownloadBase
{
	protected List<string> mNeedDownloadFileList = new();
	protected List<string> mDynamicDownloadList = new();			// 只动态下载的资源,此目录中的资源不会在更新时下载,只是在需要加载时才会下载
	protected GameDownloadCallback mProgressCallback;
	protected GameDownloadTipCallback mTipCallback;
	protected DateTime mDownloadingTimer;
	protected string mDownloadWritePath = F_PERSISTENT_ASSETS_PATH; // 默认下载到PersistentPath中
	protected int mDownloadedCount;
	protected int mDownloadSpeed;                                   // 下载速度
	protected bool mAllFinish = true;
	protected bool mNeedWritePersistentFileList;
	public void willDestroy()
	{
		// 如果在未更新完成就关闭了程序,则确保在关闭之前更新文件列表
		if (!mAllFinish)
		{
			// 确保在缓存目录有当前的版本号文件
			writeTxtFile(mDownloadWritePath + VERSION, mAssetVersionSystem.getLocalVersion());
			updateLocalFileList();
			mAllFinish = true;
		}
	}
	public void checkVersion(bool enableHotFix, GameDownloadCallback callback)
	{
		mProgressCallback = callback;
		if (isEditor() || !enableHotFix)
		{
			// 编辑器中和不下载更新的版本中不更新,直接就是全量的资源
			allFinished();
			return;
		}

		log("下载目录:" + mDownloadWritePath);
		log("资源下载地址:" + mResourceManager.getDownloadURL());
		checkAllFile();
	}
	public void setDynamicDownloadList(List<string> list) 
	{
		mDynamicDownloadList.setRange(list.safe());
	}
	public void setShowTipCallback(GameDownloadTipCallback callback) { mTipCallback = callback; }
	//------------------------------------------------------------------------------------------------------------------------------
	// 版本号文件下载完毕
	protected void checkAllFile()
	{
		mAllFinish = false;

		mTipCallback?.Invoke(DOWNLOAD_TIP.CHECKING_UPDATE);
		mProgressCallback?.Invoke(0.0f, PROGRESS_TYPE.CHECKING_UPDATE, EMPTY, 0, 0);

		// 检查是否需要更新安装包,移动端会判断是否需要重新下载整个安装包
		string remoteVersion = mAssetVersionSystem.getRemoteAssetsVersion();
		string localVersion = mAssetVersionSystem.getLocalVersion();
		VERSION_COMPARE fullCompare = compareVersion3(remoteVersion, localVersion, out _, out VERSION_COMPARE bigCompare);
		// 如果本地版本号大于远端的,则不下载,此时远端资源还未上传,本地可以直接正常运行
		// 仅限安装的是全量资源包,才能从StreamingAssets中读取,如果不是全量资源包,则无法运行,但是此处无法判断是否为全量,只能默认为全量
		if (fullCompare == VERSION_COMPARE.REMOTE_LOWER)
		{
			// 根据StreamingAssets的文件数来判断是否为全量包,为了保险起见,文件数量小于等于5个时为非全量包
			if (mAssetVersionSystem.getStreamingAssetsFile().Count <= 5)
			{
				logError("当前不是全量安装包,且本地版本号大于远端版本号,无法运行游戏");
			}
			AssetVersionSystem.setReadPathType(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
			mTipCallback?.Invoke(DOWNLOAD_TIP.NONE);
			allFinished();
			return;
		}
		// 大版本号低于远端,则提示下载安装包,但是Windows不需要提示
		if (bigCompare == VERSION_COMPARE.LOCAL_LOWER && !isWindows())
		{
			mTipCallback?.Invoke(DOWNLOAD_TIP.NONE);
			notifyNeedInstallPackage();
			return;
		}

		var streamingFiles = mAssetVersionSystem.getStreamingAssetsFile();
		var persistentFiles = mAssetVersionSystem.getPersistentAssetsFile();
		var remoteFiles = mAssetVersionSystem.getRemoteAssetsFile();
		log("本地StreamingAssets文件数量：" + streamingFiles.Count);
		log("本地PersistentAssets文件数量：" + persistentFiles.Count);
		log("远端文件数量：" + remoteFiles.Count);

		mProgressCallback?.Invoke(0.0f, PROGRESS_TYPE.DELETE_FILE, EMPTY, 0, 0);

		// 删除文件,只能删除Persistent中的文件,但是列表中的元素还是需要都删除掉
		using var a = new ListScope<string>(out var deleteFileList);
		// Persistent中需要删除列表记录,删除文件
		DateTime start = DateTime.Now;
		checkDeleteFile(persistentFiles, remoteFiles, deleteFileList);
		log("需要删除" + deleteFileList.Count + "个文件");
		foreach (string fileToDelete in deleteFileList)
		{
			persistentFiles.Remove(fileToDelete);
			string fullPath = F_PERSISTENT_ASSETS_PATH + fileToDelete;
			log("删除文件:" + fullPath);
			if (!deleteFile(fullPath))
			{
				logError("删除文件失败:" + fullPath);
			}
			mNeedWritePersistentFileList = true;
		}

		// StreamingAssets中无法删除文件,只能删除列表记录
		deleteFileList.Clear();
		checkDeleteFile(streamingFiles, remoteFiles, deleteFileList);
		foreach (string fileToDelete in deleteFileList)
		{
			streamingFiles.Remove(fileToDelete);
			deleteStreamingAssetsFile(fileToDelete);
		}

		log("删除文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");

		// 要下载的文件,其中不包含版本文件,下载新文件,每次只下载一个文件
		DateTime start1 = DateTime.Now;
		checkNeedDownloadFile(mNeedDownloadFileList, streamingFiles, persistentFiles, remoteFiles, mDynamicDownloadList);
		log("对比需要下载的文件列表耗时:" + (int)(DateTime.Now - start1).TotalMilliseconds + "毫秒");
		mNeedDownloadFileList.Remove(VERSION);
		log("需要下载" + mNeedDownloadFileList.Count + "个文件");
		mTipCallback?.Invoke(DOWNLOAD_TIP.NONE);
		if (mNeedDownloadFileList.Count == 0)
		{
			allFinished();
		}
		else
		{
			mDownloadingTimer = DateTime.Now;
			downloadFile(mDownloadedCount);
		}
	}
	// 下载普通资源文件
	protected void downloadFile(int index)
	{
		string fileName = mNeedDownloadFileList[index];
		downloadProgress(fileName, index, 0.0f);
		string fullURL = mResourceManager.getDownloadURL() + fileName;
		ResourceManager.loadAssetsFromUrl(fullURL, (byte[] bytes, string _) =>
		{
			// 单个资源文件下载完毕
			if (bytes == null)
			{
				log("下载失败! " + fileName);
				mTipCallback?.Invoke(DOWNLOAD_TIP.DOWNLOAD_FAILED);
				return;
			}
			mAssetVersionSystem.addDownloadedInfo(bytes.Length, getFileNameWithSuffix(fileName));
			// 将文件保存到本地
			writeFile(mDownloadWritePath + fileName, bytes, bytes.Length);
			onDownloaded();

			// 检查下载的文件是否正确
			if (!mAssetVersionSystem.getRemoteAssetsFile().TryGetValue(fileName, out GameFileInfo remoteInfo))
			{
				logError("已下载的文件不存在与远端文件列表, 下载的文件:" + fileName);
				mTipCallback?.Invoke(DOWNLOAD_TIP.NOT_IN_REMOTE_FILE_LIST);
				return;
			}

			GameFileInfo localInfo = new();
			localInfo.mFileName = fileName;
			localInfo.mFileSize = bytes.Length;
			localInfo.mMD5 = generateFileMD5(bytes);
			mAssetVersionSystem.getPersistentAssetsFile().set(fileName, localInfo);
			if (remoteInfo.mFileName != localInfo.mFileName ||
				remoteInfo.mFileSize != localInfo.mFileSize ||
				remoteInfo.mMD5 != localInfo.mMD5)
			{
				logError("下载的文件信息与远端的信息不一致:下载的信息:" + localInfo.mFileName + ", " + localInfo.mFileSize + ", " + localInfo.mMD5 +
						", 远端的信息:" + remoteInfo.mFileName + ", " + remoteInfo.mFileSize + ", " + remoteInfo.mMD5);
				mTipCallback?.Invoke(DOWNLOAD_TIP.VERIFY_FAILED);
			}

			// 所有文件已经下载完毕
			if (++mDownloadedCount >= mNeedDownloadFileList.Count)
			{
				allFinished();
			}
			// 还没下载完,下载下一个文件,这里延迟执行,避免可能的递归太深,导致栈溢出
			else
			{
				delayCall(() => { downloadFile(mDownloadedCount); });
			}
		}, (ulong downloaded, int downloadDelta, double deltaTimeMillis, float progress)=>
		{
			mDownloadSpeed = (int)(downloadDelta * 1000 / (float)deltaTimeMillis);
			if ((DateTime.Now - mDownloadingTimer).TotalSeconds > 1.0f)
			{
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
		mProgressCallback?.Invoke(divide(index, allCount), PROGRESS_TYPE.DOWNLOAD_RESOURCE, fileName, mDownloadSpeed, remainTime);
	}
	// 所有资源更新完毕
	protected void allFinished()
	{
		// 更新FileList文件,VERSION文件
		if (mAssetVersionSystem.getRemoteAssetsVersion() != null)
		{
			writeTxtFile(mDownloadWritePath + VERSION, mAssetVersionSystem.getRemoteAssetsVersion());
			onFinishWriteVersion();
		}
		updateLocalFileList();
		onAllFinish();

		// 游戏更新完毕
		mAllFinish = true;
		mProgressCallback?.Invoke(1.0f, PROGRESS_TYPE.FINISH, EMPTY, 0, 0);
	}
	protected virtual void updateLocalFileList()
	{
		if (mNeedWritePersistentFileList)
		{
			writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
			log("本地文件信息列表更新完毕");
		}
	}
	protected virtual void deleteStreamingAssetsFile(string fileToDelete) { }
	protected virtual void notifyNeedInstallPackage() { }
	// 默认更新Persistent目录下的文件列表
	protected virtual void onDownloaded() { mNeedWritePersistentFileList = true; }
	// 默认更新Persistent中的版本号
	protected virtual void onFinishWriteVersion() { mAssetVersionSystem.setPersistentAssetsVersion(mAssetVersionSystem.getRemoteAssetsVersion()); }
	protected virtual void onAllFinish() { }
}