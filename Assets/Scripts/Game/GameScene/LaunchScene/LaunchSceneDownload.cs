using System;
using System.Collections.Generic;
using static FileUtility;
using static GameUtility;
using static FrameBaseUtility;
using static FrameBaseDefine;
using static FrameBase;

// 下载更新资源,部分代码可自己实现
public class LaunchSceneDownload : SceneProcedure
{
	// 允许动态下载的目录列表,此列表中的文件不会打包到apk中,也不会在游戏启动时从服务器下载,而是在加载资源时才会进行下载
	public static List<string> DYNAMIC_DOWNLOAD_LIST = new()
	{
		"DynamicDownloading/",
	};
	protected GameDownload mInstance;
	protected int mRemainRetryCount = 3;		// 文件下载失败的剩余自动重试次数,没有剩余次数时将会提示玩家是否重试
	public LaunchSceneDownload()
	{
		mInstance = new GameDownload();
		// 设置动态下载的列表
		mInstance.setDynamicDownloadList(DYNAMIC_DOWNLOAD_LIST);
		mInstance.setShowTipCallback((DOWNLOAD_TIP tip) =>
		{
			if (tip == DOWNLOAD_TIP.NONE)
			{
				//mUIDownload.setDownloadInfo("");
			}
			else if (tip == DOWNLOAD_TIP.CHECKING_UPDATE)
			{
				//mUIDownload.setDownloadInfo("正在检查更新...");
			}
			else if (tip == DOWNLOAD_TIP.DOWNLOAD_FAILED)
			{
				if (mRemainRetryCount > 0)
				{
					--mRemainRetryCount;
					retry(true);
				}
				else
				{
					// 这里可选弹窗让用户选择是否重试
					//dialogYesNoResource("文件下载失败,是否重试?", retry);
					retry(true);
				}
			}
			else if (tip == DOWNLOAD_TIP.NOT_IN_REMOTE_FILE_LIST)
			{
				// 这里可选弹窗让用户选择是否重试
				//dialogYesNoResource("已下载的文件不存在与远端文件列表,是否重试?", retry);
				retry(true);
			}
			else if (tip == DOWNLOAD_TIP.VERIFY_FAILED)
			{
				if (mRemainRetryCount > 0)
				{
					--mRemainRetryCount;
					retry(true);
				}
				else
				{
					// 这里可选弹窗让用户选择是否重试
					//dialogYesNoResource("下载文件错误,是否重试?", retry);
					retry(true);
				}
			}
		});
	}
	public override void init()
	{
		// 未启用热更时可以不进行下载,webgl上全部都是远程异步加载的,也不用下载
		if (isEditor() /*|| !isEnableHotFix()*/ || isWebGL())
		{
			mInstance.skipDownload(onDownloadProgress);
		}
		else
		{
			mInstance.startCheckVersion(onDownloadProgress);
		}
	}
	public override void exit()
	{
		//mUIDownload?.close();
	}
	public override void willDestroy()
	{
		base.willDestroy();
		mInstance.willDestroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 在热更全部下载完成后,执行此函数,再启动热更.
	// 这个函数的目的是确保最新的混淆密钥文件一定存在于PersistenPath中
	// 因为在启动热更时GameHotFixBase会固定从PersistenPath中加载密钥文件
	// 如果加载的密钥文件不是最新的,则无法启动游戏
	protected void checkNeedCopySecret(Action callback)
	{
		// 如果StreamingAssets中的版本号大于PersistentData的版本号(所以这里的前提是版本号都是正确的,否则错误拷贝就会无法执行后面混淆后的代码),则需要将混淆密钥文件拷贝到PersistentData中
		// 确保PersistentData中的密钥文件肯定是最新的
		string streamVersion = mAssetVersionSystem.getStreamingAssetsVersion();
		string persistVersion = mAssetVersionSystem.getPersistentDataVersion();
		VERSION_COMPARE fullCompare = compareVersion3(streamVersion, persistVersion, out _, out _);
		logBase("streamVersion:" + streamVersion + ", persistVersion:" + persistVersion + ", fullCompare:" + fullCompare);
		logBase("isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE):" + isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE));
		if (!isEditor() && (fullCompare == VERSION_COMPARE.LOCAL_LOWER || !isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE)))
		{
			copyFileAsync(F_ASSET_BUNDLE_PATH + DYNAMIC_SECRET_FILE, F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, () =>
			{
				GameFileInfo streamingInfo = mAssetVersionSystem.getStreamingAssetsFile().get(DYNAMIC_SECRET_FILE);
				if (streamingInfo != null)
				{
					var persistAssetsFiles = mAssetVersionSystem.getPersistentAssetsFile();
					GameFileInfo persistInfo = persistAssetsFiles.get(DYNAMIC_SECRET_FILE);
					if (persistInfo == null)
					{
						persistInfo = new();
						persistAssetsFiles.add(DYNAMIC_SECRET_FILE, persistInfo);
					}
					persistInfo.mFileName = streamingInfo.mFileName;
					persistInfo.mFileSize = streamingInfo.mFileSize;
					persistInfo.mMD5 = streamingInfo.mMD5;
					// 拷贝完以后更新FileList
					writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
				}
				callback?.Invoke();
			});
		}
		else
		{
			callback?.Invoke();
		}
	}
	protected void retry(bool yes)
	{
		if (yes)
		{
			mInstance.startCheckVersion(onDownloadProgress);
		}
		else
		{
			stopApplication();
		}
	}
	protected void onDownloadProgress(float progress, PROGRESS_TYPE type, string info, int bytesPerSecond, int downloadRemainSeconds)
	{
		//mUIDownload.setProgress(progress);
		if (type == PROGRESS_TYPE.DELETE_FILE)
		{
			//mUIDownload.setDownloadInfo("正在删除无用文件");
		}
		else if (type == PROGRESS_TYPE.DOWNLOAD_RESOURCE)
		{
			//mUIDownload.setDownloadInfo("正在下载资源", false);
		}
		else if (type == PROGRESS_TYPE.FINISH)
		{
			//mUIDownload.setDownloadInfo("更新完毕,即将进入游戏...");
			checkNeedCopySecret(launch);
		}
	}
	protected void onLaunchError()
	{
		// 可选重试弹窗提示
		//dialogYesNoResource("资源加载失败,是否重试?", (bool yes) =>
		//{
		//	if (yes)
		//	{
		//		launch();
		//	}
		//	else
		//	{
		//		stopApplication();
		//	}
		//});
		launch();
	}
	protected void launch()
	{
		HybridCLRSystem.launchHotFix(getAESKeyBytes(), getAESIVBytes(),
			// 下载或者加载程序集
			(string fileName, BytesIntCallback callback) =>
			{
				// webgl下只能从远端下载资源
				if (isWebGL())
				{
					// 这里需要根据版本号自己构造出一个远端下载路径
					ObsSystem.downloadBytes(/*getRemoteFolder(mAssetVersionSystem.getRemoteVersion()) +*/ fileName, callback);
				}
				else
				{
					openFileAsync(availableReadPath(fileName), true, (byte[] bytes) => { callback?.Invoke(bytes, bytes.Length); });
				}
			},
			onLaunchError);
	}
}