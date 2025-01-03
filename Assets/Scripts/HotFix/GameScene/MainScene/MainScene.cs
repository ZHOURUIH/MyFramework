using System;

public class MainScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(MainSceneLoading);
		mExitProcedure = typeof(MainSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure(typeof(MainSceneLoading));
		addProcedure(typeof(MainSceneLogin));
		addProcedure(typeof(MainSceneGaming));
		addProcedure(typeof(MainSceneExit));
	}
}