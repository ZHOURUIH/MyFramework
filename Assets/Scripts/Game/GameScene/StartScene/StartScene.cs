
public class StartScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(StartSceneDemo);
		mExitProcedure = typeof(StartSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure<StartSceneDemo>();
		addProcedure<StartSceneExit>();
	}
}
