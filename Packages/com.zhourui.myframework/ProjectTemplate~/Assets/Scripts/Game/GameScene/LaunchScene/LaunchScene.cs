
public class LaunchScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(LaunchSceneVersion);
		mExitProcedure = typeof(LaunchSceneExit);
	}
	public override void createSceneProcedure()
	{
		addProcedure<LaunchSceneVersion>();
		addProcedure<LaunchSceneExit>();
	}
}