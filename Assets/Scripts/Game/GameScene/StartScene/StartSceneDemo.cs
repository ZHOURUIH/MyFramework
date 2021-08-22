using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StartSceneDemo : SceneProcedure
{
#if USE_ILRUNTIME
	protected float mProgress;
#endif
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		// 一般在此处加载界面,加载场景
		LT.LOAD_UGUI_SHOW(LAYOUT.DEMO);
#if USE_ILRUNTIME
		mProgress = 0.0f;
		mScriptDemo.setText("ILRuntime热更生效");
#else
		mScriptDemo.setText("非ILRuntime,请在PlayerSettings中添加USE_ILRUNTIME宏以启用ILRuntime热更");
#endif
	}
	protected override void onUpdate(float elapsedTime)
	{
#if USE_ILRUNTIME
		if (mProgress >= 0.0f && mProgress < 1.0f)
		{
			mProgress += elapsedTime * 0.5f;
			clampMax(ref mProgress, 1.0f);
			mScriptDemo.setText("ILRuntime热更生效,加载进度:" + FToS(mProgress, 2));
		}
		// 所有资源和热更代码下载完毕后,重新加载热更代码,以及热更后的资源
		if (mProgress >= 1.0f)
		{
			mILRSystem.launchILR();
			mProgress = -1.0f;
		}
#endif
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE_LAYOUT(LAYOUT.DEMO);
	}
}