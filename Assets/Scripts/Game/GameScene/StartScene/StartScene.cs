using UnityEngine;

public class StartScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(StartSceneLoading);
		mExitProcedure = typeof(StartSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure(typeof(StartSceneLoading));
		addProcedure(typeof(StartSceneDemo));
		addProcedure(typeof(StartSceneExit));
	}
}
