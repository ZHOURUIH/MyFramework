using System;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAllTable()
	{
		;
	}
	protected override void registerAll()
	{
		LayoutRegisterHotFix.registeAll();
    }
	protected override void initFrameSystem()
	{
		//registeFrameSystem<NetManager>((com) =>		{ mNetManager = com; });
	}
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}