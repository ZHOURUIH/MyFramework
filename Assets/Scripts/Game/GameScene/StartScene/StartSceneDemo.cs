using static GameBase;
using static StringUtility;

public class StartSceneDemo : SceneProcedure
{
	protected float mTime;
	public override void init()
	{
		// 一般在此处加载界面,加载场景
		CmdLayoutManagerLoad.execute(typeof(UIDemo), 0);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mTime += elapsedTime;
		mUIDemo.setText(FToS(3 - mTime, 1) + "秒后进入热更");
		if (mTime >= 3.0f)
		{
			HybridCLRSystem.launchHotFix(null, null, null);
		}
	}
	public override void exit()
	{
		mUIDemo?.close();
	}
}