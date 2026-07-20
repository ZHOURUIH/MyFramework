using System;
using static FileUtility;
using static GameUtility;
using static FrameBaseUtility;
using static FrameBaseDefine;
using static FrameBase;

// 下载更新资源,部分代码可自己实现
public class LaunchSceneDownload : SceneProcedure
{
	protected GameDownload mInstance;
	public LaunchSceneDownload()
	{
		mInstance = new GameDownload();
		mInstance.setTipCallback((DOWNLOAD_TIP tip) =>
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
				// 这里可选弹窗让用户选择是否重试
				//dialogYesNoResource("文件下载失败,是否重试?", retry);
			}
			else if (tip == DOWNLOAD_TIP.NOT_IN_REMOTE_FILE_LIST)
			{
				// 这里可选弹窗让用户选择是否重试
				//dialogYesNoResource("已下载的文件不存在与远端文件列表,是否重试?", retry);
			}
			else if (tip == DOWNLOAD_TIP.VERIFY_FAILED)
			{
				// 这里可选弹窗让用户选择是否重试
				//dialogYesNoResource("下载文件错误,是否重试?", retry);
			}
		});
	}
	public override void init()
	{
		base.init();
		mInstance.setProgressCallback(onDownloadProgress);
		// 未启用热更时可以不进行下载,webgl上全部都是远程异步加载的,也不用下载
		if (isEditor() /*|| !isEnableHotFix()*/ || isWebGL())
		{
			mInstance.skipDownload();
		}
		else
		{
			mInstance.startCheckVersion();
		}
	}
	public override void exit()
	{
		base.exit();
		//mUIDownload?.close();
	}
	public override void willDestroy()
	{
		base.willDestroy();
		mInstance.willDestroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void retry(bool yes)
	{
		if (yes)
		{
			mInstance.startCheckVersion();
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
			launch();
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
		// 下载或者加载程序集
		HybridCLRSystem.launchHotFix((string fileName, BytesIntCallback callback) =>
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
		}, onLaunchError);
	}
}