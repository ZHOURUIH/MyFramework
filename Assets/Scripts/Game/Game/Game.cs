using System;
using static FrameUtility;
using static FrameBase;
#if USE_ILRUNTIME
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
#endif

public class Game : GameFramework
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void initFrameSystem()
	{
		base.initFrameSystem();
		registeFrameSystem(typeof(BattleSystem));
	}
	protected override void init()
	{
		base.init();
		mLayoutManager.setUseAnchor(false);
#if USE_ILRUNTIME
		mILRSystem.setInitILRFunc((ILRAppDomain appdomain) =>
		{
			ILRLaunchExtension ilr = new ILRLaunchExtension();
			ilr.onILRuntimeInitialized(appdomain);
		});
#endif
	}
	protected override void notifyBase()
	{
		base.notifyBase();
		// 所有类都构造完成后通知GameBase
		GameBase.constructGameDone();
	}
	protected override void registe()
	{
		LayoutRegister.registeAllLayout();
		SQLiteRegisterMain.registeAllTable();
		PacketRegister.registeAllPacket();
	}
	protected override void launch()
	{
		base.launch();
		enterScene<StartScene>();
	}
}