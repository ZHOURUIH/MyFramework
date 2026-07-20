using System;
using static FrameBaseUtility;
using static GBR;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAllTable()
	{
        ExcelRegister.registeAll();
    }
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
	protected override void onPostInit()
	{
		if (isDevOrEditor())
		{
			HotFixTest.runAll();
		}
	}
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}