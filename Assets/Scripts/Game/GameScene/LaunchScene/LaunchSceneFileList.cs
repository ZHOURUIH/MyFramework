using static FrameBaseDefine;
using static FrameBaseUtility;
using static FrameBase;

public class LaunchSceneFileList : SceneProcedure
{
	public string mRemoteListMD5;
	public override void init()
	{
		// 这里需要自己构造一个远端路径
		mRemoteListMD5 = ObsSystem.getFileMD5(/*getRemoteFolder(mAssetVersionSystem.getRemoteVersion()) +*/ FILE_LIST);
		//mUIDownload.setDownloadInfo("正在获取资源信息...");
		// 这里根据自己的设置传是否启用热更
		bool enableHotFix = true;
		mAssetVersionSystem.startCheckFileList(enableHotFix, mRemoteListMD5, null, null, onSuccess, onFailed, checkNeedRequestRemoteFileList);
	}
	public override void exit() { }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onSuccess()
	{
		mGameSceneManager.getCurScene().changeProcedure<LaunchSceneDownload>();
	}
	protected void onFailed()
	{
		// 可选弹窗提示是否重试
		//dialogYesNoResource("最新文件列表获取失败,是否重试?", (bool ok) =>
		//{
		//	if (ok)
		//	{
		//		mAssetVersionSystem.startCheckFileList(isEnableHotFix(), mRemoteListMD5, null, null, onSuccess, onFailed, checkNeedRequestRemoteFileList);
		//	}
		//	else
		//	{
		//		stopApplication();
		//	}
		//});
		// 这里根据自己的设置传是否启用热更
		bool enableHotFix = true;
		mAssetVersionSystem.startCheckFileList(enableHotFix, mRemoteListMD5, null, null, onSuccess, onFailed, checkNeedRequestRemoteFileList);
	}
	// 对比本地和远端的文件列表,如果不一致,则将远端的文件列表下载到本地
	protected void checkNeedRequestRemoteFileList(BytesCallback callback)
	{
		// 这里需要自己构造一个远端路径
		ObsSystem.downloadBytes(/*getRemoteFolder(mAssetVersionSystem.getRemoteVersion()) +*/ FILE_LIST, (byte[] content, int _) => { callback?.Invoke(content); });
	}
}