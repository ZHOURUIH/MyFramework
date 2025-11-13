using static FrameBaseUtility;
using static FrameBaseDefine;
using static FrameBase;

public class LaunchSceneVersion : SceneProcedure
{
	protected bool mRemoteDone;
	protected bool mStreamingAndPersistDone;
	public override void init()
	{
		CmdLayoutManagerLoad.executeAsync<UIDemo>(0, () =>
		{
			if (isEditor() || !isEnableHotFix() || isWebGL())
			{
				mAssetVersionSystem.setStreamingAssetsVersion(null);
				mGameSceneManager.getCurScene().changeProcedure<LaunchSceneDownload>();
				return;
			}
			// 正在检查版本号
			//mUIDownload.setDownloadInfo("正在检查版本号...");
			doGetRemoteVersion();
			mAssetVersionSystem.loadStreamingAssetsVersionAndPersistentDataVersion(() =>
			{
				mStreamingAndPersistDone = true;
			});
		});

	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mRemoteDone && mStreamingAndPersistDone)
		{
			logBase("StreamingVersion:" + mAssetVersionSystem.getStreamingAssetsVersion() + 
					", PersistVersion:" + mAssetVersionSystem.getPersistentDataVersion() + 
					", RemoteVersion:" + mAssetVersionSystem.getRemoteVersion());
			// 需要设置自己的远端下载路径
			//mResourceManager.setDownloadURL(OBS_URL + getRemoteFolder(mAssetVersionSystem.getRemoteVersion()));
			FrameCrossParam.mDownloadURL = mResourceManager.getDownloadURL();
			mGameSceneManager.getCurScene().changeProcedure<LaunchSceneFileList>();
		}
	}
	protected void doGetRemoteVersion()
	{
		ObsSystem.downloadTxt(/*getRemoteFolder("") +*/ VERSION, (string version) =>
		{
			if (version.isEmpty())
			{
				// 可选弹窗提示是否重试
				//dialogYesNoResource("无法获取到远端服务器,是否重试?", (bool ok) =>
				//{
				//	if (ok)
				//	{
				//		doGetRemoteVersion();
				//	}
				//	else
				//	{
				//		stopApplication();
				//	}
				//});
				return;
			}
			mAssetVersionSystem.setRemoteVersion(version);
			mRemoteDone = true;
		});
	}
}