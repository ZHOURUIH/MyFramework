using static FrameBaseUtility;
using static FrameBaseDefine;
using static FrameBase;
using static FileUtility;

public class LaunchSceneVersion : SceneProcedure
{
	protected bool mRemoteDone;
	protected bool mStreamingDone;
	protected bool mPersistDone;
	public override void init()
	{
		CmdLayoutManagerLoad.execute(typeof(UIDemo), 0);

		bool enableHotFix = true;
		if (isEditor() || !enableHotFix)
		{
			mAssetVersionSystem.setStreamingAssetsVersion(null);
			mGameSceneManager.getCurScene().changeProcedure<LaunchSceneDownload>();
			return;
		}
		// 正在检查版本号
		//mUIDownload.setDownloadInfo("正在检查版本号...");
		ObsSystem.downloadTxt(/*getRemoteFolder("") +*/ VERSION, (string version) =>
		{
			mAssetVersionSystem.setRemoteVersion(version);
			mRemoteDone = true;
		});
		openTxtFileAsync(F_ASSET_BUNDLE_PATH + VERSION, !isEditor(), (string version) =>
		{
			mAssetVersionSystem.setStreamingAssetsVersion(version);
			mStreamingDone = true;
		});
		openTxtFileAsync(F_PERSISTENT_ASSETS_PATH + VERSION, false, (string version) =>
		{
			mAssetVersionSystem.setPersistentDataVersion(version);
			mPersistDone = true;
		});
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mRemoteDone && mStreamingDone && mPersistDone)
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
}