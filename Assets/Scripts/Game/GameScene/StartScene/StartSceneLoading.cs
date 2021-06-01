using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StartSceneLoading : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		// 为了让项目自己控制启动时长,所以有一部分Frame部分的资源加载由Game部分来执行
		mKeyFrameManager.loadAll(false);
		// 一般在此处加载界面,加载场景
		LT.LOAD_UGUI_SHOW(LAYOUT.DEMO_START);
	}
	protected override void onUpdate(float elapsedTime)
	{
		if(isKeyCurrentDown(KeyCode.Escape))
		{
			changeProcedure(Typeof<StartSceneDemo>());
		}
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		// 尽量在流程退出时隐藏或者卸载布局
		LT.HIDE_LAYOUT(LAYOUT.DEMO_START);
	}
}