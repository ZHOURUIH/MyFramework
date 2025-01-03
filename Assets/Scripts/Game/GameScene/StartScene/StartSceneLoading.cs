using UnityEngine;
using static FrameUtility;
using static FrameBase;

public class StartSceneLoading : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		// 一般在此处加载界面,加载场景
		LT.LOAD_SHOW<UIDemoStart>();
	}
	protected override void onUpdate(float elapsedTime)
	{
		if (mInputSystem.isKeyCurrentDown(KeyCode.Escape))
		{
			changeProcedure<StartSceneDemo>();
		}
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		// 尽量在流程退出时隐藏或者卸载布局
		LT.HIDE<UIDemoStart>();
	}
}