using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StartSceneDemo : SceneProcedure
{
	public StartSceneDemo(GameScene gameScene)
		:base(gameScene){}
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		// 一般在此处加载界面,加载场景
		LT.LOAD_UGUI_SHOW(LAYOUT.DEMO, 0);
	}
	protected override void onUpdate(float elapsedTime){}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE_LAYOUT(LAYOUT.DEMO);
	}
}