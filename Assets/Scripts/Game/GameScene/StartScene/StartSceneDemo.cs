using static FrameUtility;

public class StartSceneDemo : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		// 一般在此处加载界面,加载场景
		LT.LOAD_SHOW<UIDemo>();
		delayCall(() => { HybridCLRSystem.launchHotFix(); }, 3.0f);
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE<UIDemo>();
	}
}