using static LT;

public class MainSceneLogin : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure)
	{
		LOAD_ASYNC<UILogin>();
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		HIDE<UILogin>();
	}
}