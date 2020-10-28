using UnityEngine;
using System.Collections;

public class StartScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(StartSceneLoading);
		mExitProcedure = typeof(StartSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure(Typeof<StartSceneLoading>());
		addProcedure(Typeof<StartSceneDemo>());
		addProcedure(Typeof<StartSceneExit>());
	}
}
