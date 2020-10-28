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
		registeFrameSystem(UnityUtility.Typeof<GameConfig>());
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
		CommandGameSceneManagerEnter cmd = FrameBase.newMainCmd(out cmd, false);
		cmd.mSceneType = typeof(StartScene);
		FrameBase.pushCommand(cmd, FrameBase.mGameSceneManager);
	}
}