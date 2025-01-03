using static FrameUtility;

public class MainSceneLoading : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		changeProcedureDelay<MainSceneLogin>();
	}
	protected override void onExit(SceneProcedure nextProcedure) { }
}