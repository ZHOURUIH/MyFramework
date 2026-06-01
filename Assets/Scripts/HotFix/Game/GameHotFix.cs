using System;
using static GameDefine;
using static GBH;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAll()
	{
		LayoutRegisterHotFix.registeAll();
		PacketRegister.registeAll();
	}
	protected override void initFrameSystem()
	{
		registeFrameSystem<NetManager>((com) =>		{ mNetManager = com; });
		registeFrameSystem<DemoSystem>((com) =>		{ mDemoSystem = com; });
		registeFrameSystem<BattleSystem>((com) =>	{ mBattleSystem = com; });
	}
	protected override string getAndroidPluginBundleName() { return ANDROID_PLUGIN_BUNDLE_NAME; }
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}