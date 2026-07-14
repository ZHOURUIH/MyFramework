using System;
using static FrameBaseUtility;
using static GameDefine;
using static GBR;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAll()
	{
		LayoutRegisterHotFix.registeAll();
		PacketRegister.registeAll();
		ExcelRegister.registeAll();
    }
	protected override void initFrameSystem()
	{
		registeFrameSystem<NetManager>((com) =>		{ mNetManager = com; });
		registeFrameSystem<DemoSystem>((com) =>		{ mDemoSystem = com; });
		registeFrameSystem<BattleSystem>((com) =>	{ mBattleSystem = com; });
	}
	protected override void onPostInit()
	{
		if (isDevOrEditor())
		{
			HotFixTest.runAll();
		}
	}
    protected override string getAndroidPluginBundleName() { return ANDROID_PLUGIN_BUNDLE_NAME; }
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}