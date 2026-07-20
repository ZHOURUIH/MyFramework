using static FrameBaseUtility;
using static FrameBase;

public class Game : GameFramework
{
	public override void init()
	{
		mOnInitFrameSystem += gameInitFrameSystem;
		mOnRegisteStuff += gameRegiste;

		base.init();
		// 编辑器中或者非热更版就强制从StreamingAssets中读取资源
		if (!isEnableHotFix() || isEditor() || isWebGL())
		{
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
		}
		else
		{
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.SAME_TO_REMOTE);
		}
		mGameSceneManager.enterScene<LaunchScene>();
	}
	//-------------------------------------------------------------------------------------------------------------
	protected void gameInitFrameSystem(){}
	protected void gameRegiste()
	{
		LayoutRegister.registeAllLayout();
	}
}