using static FrameBaseUtility;
using static FrameBase;
using static GameUtility;
using static GameDefine;

public class Game : GameFramework
{
	public override void init()
	{
		mOnInitFrameSystem += gameInitFrameSystem;
		mOnRegisteStuff += gameRegiste;
		// 这里填写自己的安卓插件包名
		mOnPackageName += () => { return ANDROID_PLUGIN_BUNDLE_NAME; };

		base.init();
		// 编辑器中或者非热更版就强制从StreamingAssets中读取资源
		if (!isHotFixEnable() || isEditor())
		{
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
		}
		else
		{
			mAssetVersionSystem.setAssetReadPath(ASSET_READ_PATH.SAME_TO_REMOTE);
		}
		mGameSceneManager.enterScene<StartScene>();
	}
	//-------------------------------------------------------------------------------------------------------------
	protected void gameInitFrameSystem(){}
	protected void gameRegiste()
	{
		LayoutRegister.registeAllLayout();
	}
}