using UnityEngine;
using System.Collections;

public class StartScene : GameScene
{
	public StartScene(string name)
		:base(name){}
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(StartSceneLoading);
		mExitProcedure = typeof(StartSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure<StartSceneLoading>();
		addProcedure<StartSceneDemo>();
		addProcedure<StartSceneExit>();
	}
}
