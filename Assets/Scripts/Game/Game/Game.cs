using static FrameUtility;
using static FrameBase;
using static GameUtility;
using static GameDefine;

public class Game : GameFramework
{
	public override void Awake()
	{
		mOnInitFrameSystem += gameInitFrameSystem;
		mOnRegisteStuff += gameRegiste;
		// 这里填写自己的安卓插件包名
		mOnPackageName += () => { return ANDROID_PLUGIN_BUNDLE_NAME; };

		base.Awake();
		// 编辑器中或者不下载资源时就强制从StreamingAssets中读取资源
		if (!isHotFixEnable())
		{
			AssetVersionSystem.setReadPathType(ASSET_READ_PATH.STREAMING_ASSETS_ONLY);
		}
		mScreenOrientationSystem.setAndroidOrientation(ANDROID_ORIENTATION.SCREEN_ORIENTATION_SENSOR_LANDSCAPE);
		enterScene<StartScene>();
	}
	//-------------------------------------------------------------------------------------------------------------
	protected void gameInitFrameSystem(){}
	protected void gameRegiste()
	{
		LayoutRegister.registeAllLayout();
	}
}