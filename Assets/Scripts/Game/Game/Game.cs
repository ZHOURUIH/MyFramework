using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Game : GameFramework
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void initFrameSystem()
	{
		base.initFrameSystem();
		registeFrameSystem(typeof(BattleSystem));
		registeFrameSystem(typeof(NetManager));
	}
	protected override void init()
	{
		base.init();
		FrameBase.mLayoutManager.setUseAnchor(false);
	}
	protected override void notifyBase()
	{
		base.notifyBase();
		// 所有类都构造完成后通知GameBase
		FrameBase.constructGameDone();
	}
	protected override void registe()
	{
		LayoutRegister.registeAllLayout();
		SQLiteRegister.registeAllTable();
		PacketRegister.registeAllPacket();
	}
	protected override void launch()
	{
		base.launch();
		FrameUtility.enterScene<StartScene>();
	}
}