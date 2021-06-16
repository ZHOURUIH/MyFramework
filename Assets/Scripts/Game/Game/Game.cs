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
	}
	protected override void init()
	{
		base.init();
		// 如果是开发移动端,需要设置SimulateTouch为true
		FrameBase.mGlobalTouchSystem.setSimulateTouch(false);
		FrameBase.mLayoutManager.setUseAnchor(false);
	}
	protected override void notifyBase()
	{
		base.notifyBase();
		// 所有类都构造完成后通知GameBase
		GameBase frameBase = new GameBase();
		frameBase.notifyConstructDone();
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